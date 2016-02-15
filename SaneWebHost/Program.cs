using SaneWeb.Controller;
using SaneWeb.Web;
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
            SaneServer ws = new SaneServer("http://+:8080/");
            ws.addController(typeof(Controller));
            ws.run();
            Console.WriteLine("Webserver running!");
            Console.ReadKey();
            ws.stop();
        }
    }
}
