using System;
using System.Net;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SaneWeb.Web
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
    public class WSPacket
    {
        public byte[] data { get; set; }
        public String text
        {
            get
            {
                return Encoding.ASCII.GetString(data);
            }
        }
        public WSOpCode opCode { get; set; }
        public WSPacket(byte[] data, WSOpCode opCode)
        {
            this.data = data;
            this.opCode = opCode;
        }
    }

    public enum WSOpCode
    {
        CONTINUATION = 0x00,
        TEXT = 0x01,
        BINARY = 0x02,
        CLOSE = 0x08,
        PING = 0x09,
        PONG = 0x0A
    }
}
