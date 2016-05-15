using SaneWeb.Resources;
using SaneWeb.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using WebSocketSharp;

namespace SocketExample
{
    class Program
    {
        static void Main(string[] args)
        {
            SaneServer ws = new SaneServer(
    (Utility.fetchFromResource(true, Assembly.GetExecutingAssembly(), "SocketExample.Resources.ViewStructure.xml")),
    "Database\\SaneDB.db", false,
    "http://+:80/");
            ws.setShowPublicErrors(true);
            ws.addWebSocketService<EchoWebSocketService>(8080, "/Echo");
            ws.run();
            Console.WriteLine("Running!");
            Console.ReadLine();
        }
    }
    public class EchoWebSocketService : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Send(e.Data);
        }
    }
}
