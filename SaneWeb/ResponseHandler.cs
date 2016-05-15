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
            MethodInfo[][] info = controllers.Select((g) => (g.GetMethods().Where((x) => (x.GetCustomAttribute<ControllerAttribute>() != null)).ToArray())).ToArray();
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
            bool flag = false;
            foreach (MethodInfo method in methods)
            {
                ControllerAttribute attribute = method.GetCustomAttribute<ControllerAttribute>();
                if (attribute == null)
                {
                    continue;
                }
                String trimmed = context.Request.RawUrl.Substring(0, context.Request.RawUrl.LastIndexOf("/") + 1);
                if ((attribute.path.Substring(0).Equals(trimmed)))
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
                        if (sender.getShowPublicErrors())
                        {
                            returned = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e));
                        }
                        else
                        {
                            returned = Encoding.UTF8.GetBytes("An error occured processing your request!");
                        }
                    }
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                XmlDocument structure = sender.getViewStructure();
                String xPath = "view/resource";
                XmlNodeList resources = structure.SelectNodes(xPath);
                String request = context.Request.RawUrl.Substring(1).Replace("/", ".");
                String homePage = String.Empty;
                String notFound = String.Empty;
                foreach (XmlNode node in resources)
                {
                    if (request.Equals(node.Attributes["path"].Value.Replace("/", ".")))
                    {
                        context.Response.ContentType = node.Attributes["content-type"].Value;
                        byte[] clientData = Utility.fetchForClient(assembly, node.Attributes["location"].Value);
                        foreach (MethodInfo method in methods)
                        {
                            DataBoundViewAttribute attribute = method.GetCustomAttribute<DataBoundViewAttribute>();
                            String trimmed = context.Request.RawUrl.Substring(0, context.Request.RawUrl.LastIndexOf("/") + 1);
                            if ((attribute.path.Substring(0).Equals(trimmed)))
                            {
                                Object binding = method.Invoke(null, new object[] { context });
                                DataBoundView boundView = new DataBoundView(clientData, binding);
                                clientData = Encoding.UTF8.GetBytes(boundView.html);
                                break;
                            }
                        }
                        return clientData;
                    }
                    if (node.Attributes["situational"] != null)
                    {
                        String value = node.Attributes["situational"].Value;
                        if (value.Equals("homepage"))
                        {
                            homePage = node.Attributes["location"].Value;
                        }
                        else if (value.Equals("404"))
                        {
                            notFound = node.Attributes["location"].Value;
                        }
                    }
                }
                if (context.Request.RawUrl.Trim().Length <= 1)
                {
                    context.Response.ContentType = "text/html";
                    byte[] clientData = Utility.fetchForClient(assembly, homePage.Replace("/", "."));
                    foreach (MethodInfo method in methods)
                    {
                        DataBoundViewAttribute attribute = method.GetCustomAttribute<DataBoundViewAttribute>();
                        String trimmed = context.Request.RawUrl.Substring(0, context.Request.RawUrl.LastIndexOf("/") + 1);
                        if ((attribute.path.Substring(0).Equals(trimmed)))
                        {
                            Object binding = method.Invoke(null, new object[] { context });
                            DataBoundView boundView = new DataBoundView(clientData, binding);
                            clientData = Encoding.UTF8.GetBytes(boundView.html);
                            break;
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
                byte[] ret = Utility.fetchForClient(assembly, notFound.Replace("/", "."));
                foreach (MethodInfo method in methods)
                {
                    DataBoundViewAttribute attribute = method.GetCustomAttribute<DataBoundViewAttribute>();
                    String trimmed = context.Request.RawUrl.Substring(0, context.Request.RawUrl.LastIndexOf("/") + 1);
                    if ((attribute.path.Substring(0).Equals(trimmed)))
                    {
                        Object binding = method.Invoke(null, new object[] { context });
                        DataBoundView boundView = new DataBoundView(ret, binding);
                        ret = Encoding.UTF8.GetBytes(boundView.html);
                        break;
                    }
                }
                return ret;
            }
            return returned;
        }
    }
}
