using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace SaneWeb.Sockets
{
    public class WebSocketServerWrapper
    {
        private WebSocketServer server;
        public WebSocketServerWrapper(int port)
        {
            server = new WebSocketServer(port);
        }
        public void start()
        {
            server.Start();
        }
        public void stop()
        {
            server.Stop();
        }
        public void addService<T>(String path) where T : WebSocketBehavior, new()
        {
            server.AddWebSocketService<T>(path);
        }
    }
}
