using Newtonsoft.Json;
using SaneWeb.Resources;
using SaneWeb.Resources.Attributes;
using SaneWeb.Web;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SaneWeb.ViewProcessor;

namespace SaneWeb
{
    public static class ResponseHandler
    {
        /// <summary>
        /// Gets a byte representation of the data requested in this HttpListenerContext request
        /// </summary>
        /// <param name="sender">Sender of the call</param>
        /// <param name="context">Context of the HTTP request</param>
        /// <param name="controllers">Relevant controllers to be searched for methods</param>
        /// <returns>a byte representation of the data requested in this HttpListenerContext request</returns>
        public static byte[] handleResponse(SaneServer sender, HttpListenerContext context, List<Type> controllers)
        {
            List<HttpArgument> arguments = new List<HttpArgument>();
            MethodInfo[][] info = controllers.Select((g) => (g.GetMethods().Where((x) => (x.GetCustomAttribute<ControllerAttribute>() != null || x.GetCustomAttribute<DataBoundViewAttribute>() != null)).ToArray())).ToArray();
            List<MethodInfo> methods = new List<MethodInfo>();
            for (int i = 0; i < info.Length; i++)
            {
                for (int j = 0; j < info[i].Length; j++)
                {
                    methods.Add(info[i][j]);
                }
            }
            foreach (String key in context.Request.QueryString.AllKeys)
            {
                arguments.Add(new HttpArgument(key, context.Request.QueryString.GetValues(key).First()));
            }
            byte[] returned = new byte[] { };
            foreach (MethodInfo method in methods)
            {
                ControllerAttribute attribute = method.GetCustomAttribute<ControllerAttribute>();
                if (attribute == null)
                {
                    continue;
                }
                String trimmed = context.Request.RawUrl.Substring(0, context.Request.RawUrl.LastIndexOf("/") + 1);
                if ((attribute.path.Substring(0).Equals(trimmed)) && (attribute.verb.Equals(context.Request.HttpMethod)))
                {
                    try
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        Dictionary<ParameterInfo, String> match = new Dictionary<ParameterInfo, String>();
                        foreach (ParameterInfo parameter in parameters)
                        {
                            foreach (HttpArgument argument in arguments)
                            {
                                if (argument.key.Equals(parameter.Name))
                                {
                                    match[parameter] = argument.value;
                                }
                            }
                        }
                        object[] finalized = new object[parameters.Length];
                        finalized[0] = context;
                        if (context.Request.HasEntityBody)
                        {
                            finalized[1] = new StreamReader(context.Request.InputStream).ReadToEnd();
                        }
                        else
                        {
                            finalized[1] = String.Empty;
                        }
                        for (int i = 2; i < parameters.Length; i++)
                        {
                            finalized[i] = (match.ContainsKey(parameters[i])) ? match[parameters[i]] : null;
                        }
                        context.Response.ContentType = "application/json";
                        returned = Encoding.UTF8.GetBytes(method.Invoke(null, finalized) + "");
                    }
                    catch (Exception e)
                    {
                        context.Response.ContentType = "application/json";
                        if (sender.GetErrorHandler() != null)
                        {
                            context.Response.StatusCode = 400;
                            SaneErrorEventArgs args = new SaneErrorEventArgs(ResponseErrorReason.INTERNAL_API_THROW, e, false, "");
                            sender.GetErrorHandler()(null, args);
                            if (args.Propogate)
                            {
                                return Encoding.UTF8.GetBytes(args.Response);
                            }
                            else
                            {
                                return new byte[] { };
                            }
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                            return Encoding.UTF8.GetBytes("An error occured processing your request, and no error handler is currently set!");
                        }
                    }
                    return returned;
                }
                else if ((attribute.path.Substring(0).Equals(trimmed)))
                {
                    if (sender.GetErrorHandler() != null)
                    {
                        context.Response.StatusCode = 400;
                        SaneErrorEventArgs args = new SaneErrorEventArgs(ResponseErrorReason.INCORRECT_API_TYPE, new Exception("Incorrect API request type! Got " + context.Request.HttpMethod + ", expected " + attribute.verb + "!"), false, "");
                        sender.GetErrorHandler()(null, args);
                        if (args.Propogate)
                        {
                            return Encoding.UTF8.GetBytes(args.Response);
                        }
                        else
                        {
                            return new byte[] { };
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        return Encoding.UTF8.GetBytes("An error occured processing your request, and no error handler is currently set!");
                    }
                }
            }
            Assembly assembly = Assembly.GetEntryAssembly();
            XmlDocument structure = sender.GetViewStructure();
            XmlNodeList resources = structure.SelectNodes("view/resource");
            String request = context.Request.RawUrl.Substring(1).Replace("/", ".");
            XmlNode homePage = null;
            XmlNode notFound = null;
            foreach (XmlNode node in resources)
            {
                if (request.Equals(node.Attributes["path"].Value.Replace("/", ".")))
                {
                    context.Response.ContentType = node.Attributes["content-type"].Value;
                    byte[] clientData = Utility.fetchForClient(assembly, node.Attributes["location"].Value);
                    foreach (MethodInfo method in methods)
                    {
                        DataBoundViewAttribute attribute = method.GetCustomAttribute<DataBoundViewAttribute>();
                        if (attribute == null) continue;
                        if ((attribute.path.Equals(context.Request.RawUrl)))
                        {
                            Object binding = method.Invoke(null, new object[] { context });
                            DataBoundView boundView = new DataBoundView(clientData, binding);
                            return Encoding.UTF8.GetBytes(boundView.html);
                        }
                    }
                    return clientData;
                }
                if (node.Attributes["situational"] != null)
                {
                    String value = node.Attributes["situational"].Value;
                    if (value.Equals("homepage"))
                    {
                        homePage = node;
                    }
                    else if (value.Equals("404"))
                    {
                        notFound = node;
                    }
                }
            }
            if (context.Request.RawUrl.Trim().Length <= 1)
            {
                context.Response.ContentType = "text/html";
                byte[] clientData = new byte[] { };
                if (homePage != null)
                {
                    clientData = Utility.fetchForClient(assembly, homePage.Attributes["location"].Value.Replace("/", "."));
                    foreach (MethodInfo method in methods)
                    {
                        DataBoundViewAttribute attribute = method.GetCustomAttribute<DataBoundViewAttribute>();
                        if (attribute == null) continue;
                        if ((attribute.path.Substring(1).Equals(homePage.Attributes["path"].Value)))
                        {
                            Object binding = method.Invoke(null, new object[] { context });
                            DataBoundView boundView = new DataBoundView(clientData, binding);
                            return Encoding.UTF8.GetBytes(boundView.html);
                        }
                    }
                }
                return clientData;
            }
            if (sender.fileAccessPermitted)
            {
                String[] dir = context.Request.RawUrl.Substring(1).Split('/');
                if (dir.Length != 0)
                {
                    String current = Environment.CurrentDirectory;
                    for (int i = 0; i < dir.Length - 1; i++)
                    {
                        if (Directory.Exists(Path.Combine(current, dir[i])))
                        {
                            current = Path.Combine(current, dir[i]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (File.Exists(Path.Combine(current, dir[dir.Length - 1])))
                    {
                        context.Response.ContentType = "text/html";
                        return File.ReadAllBytes(Path.Combine(current, dir[dir.Length - 1]));
                    }
                }
            }
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/html";
            byte[] ret = new byte[] { };
            if (notFound != null)
            {
                ret = Utility.fetchForClient(assembly, notFound.Attributes["location"].Value.Replace("/", "."));
                foreach (MethodInfo method in methods)
                {
                    DataBoundViewAttribute attribute = method.GetCustomAttribute<DataBoundViewAttribute>();
                    if (attribute == null) continue;
                    if ((attribute.path.Substring(1).Equals(notFound.Attributes["path"].Value)))
                    {
                        Object binding = method.Invoke(null, new object[] { context });
                        DataBoundView boundView = new DataBoundView(ret, binding);
                        return Encoding.UTF8.GetBytes(boundView.html);
                    }
                }
            }
            return ret;
        }
    }
}
