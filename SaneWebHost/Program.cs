using SaneWeb.Controller;
using SaneWeb.Web;
using SaneWebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaneWebHost
{
    class Program
    {
        static void Main(string[] args)
        {
            SaneServer ws = new SaneServer("Database\\SaneDB.db", "http://+:8080/");
            ws.addController(typeof(Controller));
            ws.loadModel(typeof(Sessions));
            ws.run();
            Console.WriteLine("Webserver running!");
            Console.ReadKey();
            ws.stop();
        }
    }
}
