using SaneWeb.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using WebSocketSharp.Server;

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
        /// Boolean value determining whether or not to display exceptions publicly on errors
        /// </summary>
        private bool showPublicErrors { get; set; }

        /// <summary>
        /// XmlDocument storing information on the ViewStructure (specified in constructor)
        /// </summary>
        private XmlDocument viewStructure { get; set; }

        /// <summary>
        /// Boolean value determining whether or not to search the filesystem (relative location of executable) if file is not specified in ViewStructure
        /// </summary>
        internal bool fileAccessPermitted { get; }

        /// <summary>
        /// Creates an instance of SaneServer, initializing basic HTTP server functionality, and binds the server to specified prefixes
        /// </summary>
        /// <param name="viewStructureContent">Raw XML data to be processed as the ViewStructure</param>
        /// <param name="databasePath">Path to the SQLite DB (optional)</param>
        /// <param name="allowFileAccess">Boolean value determining whether or not to search the filesystem (relative location of executable) if file is not specified in ViewStructure (defaults to false)</param>
        /// <param name="prefixes">List of prefixes to bind the HTTP server to</param>
        public SaneServer(String viewStructureContent, String databasePath = "Database\\SaneDB.db", bool allowFileAccess = false, params string[] prefixes)
        {
            if (!HttpListener.IsSupported) throw new NotSupportedException("Requires Windows XP SP2, Server 2003 or later.");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            controllers = new List<Type>();
            models = new List<Type>();
            Directory.CreateDirectory(databasePath.Substring(0, databasePath.LastIndexOf('\\')));
            this.databasePath = databasePath;
            this.showPublicErrors = false;
            this.fileAccessPermitted = allowFileAccess;
            viewStructure = new XmlDocument();
            viewStructure.LoadXml(viewStructureContent);
            _listener.Start();
        }

        /// <summary>
        /// Gets the showPublicErrors variable
        /// </summary>
        /// <returns>Whether or not to display errors publically, handled by ResponseHandler</returns>
        public bool getShowPublicErrors()
        {
            return showPublicErrors;
        }

        /// <summary>
        /// Gets the XMLDocument representing the current view structure
        /// </summary>
        /// <returns>An XmlDocument representing the current view structure</returns>
        public XmlDocument getViewStructure()
        {
            return viewStructure;
        }

        /// <summary>
        /// Adds a controller (static class expected).
        /// </summary>
        /// <param name="controller">Type of the controller to be added</param>
        public void addController(Type controller)
        {
            if (controller.IsAbstract && controller.GetConstructors(System.Reflection.BindingFlags.Public).Length == 0)
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
        public ListDBHook<T> loadModel<T>() where T : Model<T>
        {
            Type model = typeof(T);
            if (!models.Contains(model))
            {
                if (!DBReferences.databaseOpen(databasePath))
                {
                    DBReferences.openDatabase(databasePath);
                }
                if (!DBReferences.tableExists<T>(databasePath))
                {
                    DBReferences.createTable<T>(databasePath);
                }
                models.Add(model);
            }
            return DBReferences.openTable<T>(databasePath);
        }

        /// <summary>
        /// Removes a controller from the stored controller list
        /// </summary>
        /// <param name="controller">Type of the controller to remove</param>
        public void removeController(Type controller)
        {
            if (controllers.Contains(controller))
            {
                controllers.Remove(controller);
            }
        }

        /// <summary>
        /// Starts the main responder loop for the HTTP server, starts handling live requests
        /// </summary>
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
                                Object rstr = ResponseHandler.handleResponse(this, ctx, controllers);
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

        /// <summary>
        /// Stops and closes this HTTP server
        /// </summary>
        public void stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
