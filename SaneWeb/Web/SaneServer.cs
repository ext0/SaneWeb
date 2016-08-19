using SaneWeb.Data;
using SaneWeb.Resources.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        /// <summary>
        /// HttpListener object used to handle HTTP requests
        /// </summary>
        internal readonly HttpListener _listener = new HttpListener();

        /// <summary>
        /// List of static controllers to search for API endpoints + view controllers in
        /// </summary>
        private List<Type> controllers { get; set; }

        /// <summary>
        /// List of models to be cached
        /// </summary>
        private List<Type> models { get; set; }

        /// <summary>
        /// Relative path for the SQLite DB
        /// </summary>
        private String databasePath { get; set; }

        /// <summary>
        /// XmlDocument storing information on the ViewStructure (specified in constructor)
        /// </summary>
        private XmlDocument viewStructure { get; set; }

        /// <summary>
        /// Function representing the error handler, called whenever ResponseHandler encounters a fatal error, allows user to propogate data or handle errors
        /// </summary>
        internal Action<Object, SaneErrorEventArgs> errorHandler { get; set; }

        /// <summary>
        /// Creates an instance of SaneServer, initializing basic HTTP server functionality, and binds the server to specified prefixes
        /// </summary>
        /// <param name="viewStructureContent">Raw XML data to be processed as the ViewStructure</param>
        /// <param name="databasePath">Path to the SQLite DB (optional)</param>
        /// <param name="prefixes">List of prefixes to bind the HTTP server to</param>
        private SaneServer(String viewStructureContent, String databasePath = "Database\\SaneDB.db", params string[] prefixes)
        {
            if (!HttpListener.IsSupported) throw new NotSupportedException("Requires Windows XP SP2, Server 2003 or later.");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            controllers = new List<Type>();
            models = new List<Type>();
            Directory.CreateDirectory(databasePath.Substring(0, databasePath.LastIndexOf('\\')));
            this.databasePath = databasePath;
            this.errorHandler = null;
            viewStructure = new XmlDocument();
            viewStructure.LoadXml(viewStructureContent);
            _listener.Start();
        }

        public static SaneServer CreateServer(SaneServerConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            return new SaneServer(config.ResourceXML, config.DBPath, config.Prefixes);
        }

        /// <summary>
        /// Gets the currently set error handler to consume internal API or response errors
        /// </summary>
        /// <returns>The currently set error handler to consume internal API or response errors</returns>
        public Action<Object, SaneErrorEventArgs> GetErrorHandler()
        {
            return errorHandler;
        }

        /// <summary>
        /// Sets the current error handler function to be called upon internal API or response errors
        /// </summary>
        /// <param name="errorHandler">Function to be called</param>
        public void SetErrorHandler(Action<Object, SaneErrorEventArgs> errorHandler)
        {
            this.errorHandler = errorHandler;
        }

        /// <summary>
        /// Gets the XMLDocument representing the current view structure
        /// </summary>
        /// <returns>An XmlDocument representing the current view structure</returns>
        public XmlDocument GetViewStructure()
        {
            return viewStructure;
        }

        /// <summary>
        /// Adds a controller (static class expected).
        /// </summary>
        /// <param name="controller">Type of the controller to be added</param>
        public void AddController(Type controller)
        {
            if (!(controller.IsAbstract && controller.GetConstructors(System.Reflection.BindingFlags.Public).Length == 0))
            {
                throw new Exception("Invalid type " + controller.Name + ", expected static controller class!");
            }
            if (!controllers.Contains(controller))
            {
                controllers.Add(controller);
            }
        }

        /// <summary>
        /// Loads a model of type T into the database, creating/loading relevant tables
        /// </summary>
        /// <typeparam name="T">Type of the Model to be added (must implement Model)</typeparam>
        /// <returns>A wrapper for the table created/loaded</returns>
        public ListDBHook<T> LoadModel<T>() where T : Model<T>
        {
            Type model = typeof(T);
            if (!models.Contains(model))
            {
                if (!DBReferences.DatabaseOpen(databasePath))
                {
                    DBReferences.OpenDatabase(databasePath);
                }
                if (!DBReferences.TableExists<T>(databasePath))
                {
                    DBReferences.CreateTable<T>(databasePath);
                }
                models.Add(model);
            }
            return DBReferences.OpenTable<T>(databasePath);
        }

        /// <summary>
        /// Removes a controller from the stored controller list
        /// </summary>
        /// <param name="controller">Type of the controller to remove</param>
        public void RemoveController(Type controller)
        {
            if (controllers.Contains(controller))
            {
                controllers.Remove(controller);
            }
        }

        /// <summary>
        /// Starts the main responder loop for the HTTP server, starts handling live requests
        /// </summary>
        public void Run()
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
                                Object rstr = ResponseHandler.handleResponse(this, ctx, controllers);
                                byte[] buf = (byte[])rstr;
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch (Exception e)
                            {
                                if (errorHandler == null)
                                {
                                    return;
                                }
                                GetErrorHandler()(this, new SaneErrorEventArgs(ResponseErrorReason.WEBSERVER_ERROR, e, false, ""));
                            }
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

        /// <summary>
        /// Stops and closes this HTTP server
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
