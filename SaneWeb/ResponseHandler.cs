using Newtonsoft.Json;
using SaneWeb.Resources;
using SaneWeb.Resources.Attributes;
using SaneWeb.Resources.SaneWeb.Resources.Arguments;
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
using SaneWeb.LuaEngine;
using System.Xml;

namespace SaneWeb
{
    public static class ResponseHandler
    {
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
            LuaProcesser processor;
            foreach (MethodInfo method in methods)
            {
                ControllerAttribute attribute = method.GetCustomAttribute<ControllerAttribute>();
                String trimmed = context.Request.RawUrl.Substring(0, context.Request.RawUrl.LastIndexOf("/") + 1);
                if ((attribute.path.Substring(1).Equals(trimmed)))
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
                        if (node.Attributes["lua"] != null && node.Attributes["lua"].Value.Equals("true"))
                        {
                            processor = new LuaProcesser(context, Utility.fetchForClient(assembly, node.Attributes["location"].Value));
                            return processor.processedHTML;
                        }
                        else
                        {
                            return Utility.fetchForClient(assembly, node.Attributes["location"].Value);
                        }
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
                    processor = new LuaProcesser(context, Utility.fetchForClient(assembly, homePage.Replace("/", ".")));
                    return processor.processedHTML;
                }
                context.Response.StatusCode = 404;
                context.Response.ContentType = "text/html";
                processor = new LuaProcesser(context, Utility.fetchForClient(assembly, notFound.Replace("/", ".")));
                return processor.processedHTML;
            }
            return returned;
        }
    }
}
