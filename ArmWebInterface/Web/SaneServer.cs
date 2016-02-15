using SaneWeb.Data;
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
        private List<Type> models;
        private String databasePath;

        public SaneServer(String databasePath = "Database\\SaneDB.db", params string[] prefixes)
        {
            if (!HttpListener.IsSupported) throw new NotSupportedException("Requires Windows XP SP2, Server 2003 or later.");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = ResponseHandler.handleResponse;
            controllers = new List<Type>();
            models = new List<Type>();
            this.databasePath = databasePath;
            _listener.Start();
        }

        public void addController(Type controller)
        {
            if (!controllers.Contains(controller))
            {
                controllers.Add(controller);
            }
        }

        public ListDBHook<T> loadModel<T>() where T : Model<T>
        {
            Type model = typeof(T);
            if (!models.Contains(model))
            {
                if (!DBReferences.databaseOpen(databasePath))
                {
                    DBReferences.openDatabase(databasePath);
                    if (!DBReferences.tableExists<T>(databasePath))
                    {
                        DBReferences.createTable<T>(databasePath);
                    }
                }
                models.Add(model);
            }
            return DBReferences.openTable<T>(databasePath);
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
                            catch { }
                            finally
                            {
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { }
            });
        }

        public void stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
