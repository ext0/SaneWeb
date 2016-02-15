using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaneWeb.Web
{
    public class SaneServer
    {
        public readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerContext, List<Type>, String> _responderMethod;
        private List<Type> controllers;

        public SaneServer(params string[] prefixes)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");

            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = ResponseHandler.handleResponse;
            controllers = new List<Type>();
            _listener.Start();
        }

        public void addController(Type controller)
        {
            if (!controllers.Contains(controller))
            {
                controllers.Add(controller);
            }
        }

        public void removeController(Type controller)
        {
            if (controllers.Contains(controller))
            {
                controllers.Remove(controller);
            }
        }

        public void run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                string rstr = _responderMethod(ctx, controllers);
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ControllerAttribute : Attribute
    {
        public String path;
        public ControllerAttribute(String path)
        {
            this.path = path;
        }
    }
}
