using SaneWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SaneWeb.Web
{
    public class SaneServer
    {
        public readonly HttpListener _listener = new HttpListener();
        private readonly Func<SaneServer, HttpListenerContext, List<Type>, Object> _responderMethod;
        private List<Type> controllers;
        private List<Type> models;
        private String databasePath;
        private String homePage;
        private bool showPublicErrors;
        private XmlDocument viewStructure;

        public SaneServer(String viewStructureContent, String databasePath = "Database\\SaneDB.db", params string[] prefixes)
        {
            if (!HttpListener.IsSupported) throw new NotSupportedException("Requires Windows XP SP2, Server 2003 or later.");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = ResponseHandler.handleResponse;
            controllers = new List<Type>();
            models = new List<Type>();
            this.databasePath = databasePath;
            this.showPublicErrors = false;
            viewStructure = new XmlDocument();
            viewStructure.LoadXml(viewStructureContent);
            _listener.Start();
        }

        public bool getShowPublicErrors()
        {
            return showPublicErrors;
        }

        public void setShowPublicErrors(bool showPublicErrors)
        {
            this.showPublicErrors = showPublicErrors;
        }

        public XmlDocument getViewStructure()
        {
            return viewStructure;
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

        public void setHomepage(String URI)
        {
            homePage = URI;
        }

        public String getHomepage()
        {
            return homePage;
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
                                Object rstr = _responderMethod(this, ctx, controllers);
                                byte[] buf = (byte[])rstr;
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
