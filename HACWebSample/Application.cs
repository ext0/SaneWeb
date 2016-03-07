using HACWeb.Controllers;
using SaneWeb.Data;
using SaneWeb.Resources;
using SaneWeb.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HACWeb
{
    class Application
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SaneServer ws = new SaneServer(
                (Utility.fetchFromResource(true, Assembly.GetExecutingAssembly(), "HACWeb.Resources.ViewStructure.xml")),
                "Database\\SaneDB.db",
                "http://+:80/");
            ws.setShowPublicErrors(true);
            Console.WriteLine("Initialized!");

            ws.addController(typeof(Controller));
            Console.WriteLine("Controller added!");

            ws.run();
            stopwatch.Stop();
            Console.WriteLine("[Completed in " + stopwatch.ElapsedMilliseconds + "ms] Webserver running!");
            Console.ReadKey();
            ws.stop();
        }
    }
}
