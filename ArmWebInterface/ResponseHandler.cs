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

namespace SaneWeb
{
    public static class ResponseHandler
    {
        public static string handleResponse(SaneServer sender, HttpListenerContext context, List<Type> controllers)
        {
            if (context.Response.Cookies.Count == 0)
            {
                context.Response.AddHeader("Set-Cookie", "session=" + Utility.trustedRandomString(32));
            }
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
            Object returned = String.Empty;
            bool flag = false;
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
                        for (int i = 1; i < parameters.Length; i++)
                        {
                            finalized[i] = (match.ContainsKey(parameters[i])) ? match[parameters[i]] : null;
                        }
                        returned = method.Invoke(null, finalized);
                    }
                    catch (Exception e)
                    {
                        if (e is TargetParameterCountException)
                        {
                            returned = "An error occured due to a malformed request! Please verify parameters.";
                        }
                        else
                        {
                            returned = "An unknown error occured processing your request! [" + e.Message + "]";
                        }
                    }
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                String nameSpace = String.Empty;
                if (context.Request.RawUrl.Trim().Length <= 1)
                {
                    return fetchFromResource(assembly, sender.getHomepage());
                }
                foreach (String s in assembly.GetManifestResourceNames())
                {
                    int subset = s.IndexOf(".View.") + 6;
                    if (s.Substring(subset) == context.Request.RawUrl.Substring(1).Replace("/","."))
                    {
                         return fetchFromResource(assembly, s);
                    }
                    nameSpace = s;
                }
                context.Response.StatusCode = 404;
                returned = fetchFromResource(assembly, nameSpace.Substring(0, nameSpace.IndexOf(".")) + ".View.404.html");
            }
            return returned.ToString();
        }
        public static String fetchFromResource(Assembly assembly, String resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
