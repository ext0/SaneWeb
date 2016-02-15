using SaneWeb.Controller;
using SaneWeb.Data;
using SaneWeb.Web;
using SaneWebHost.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaneWebHost
{
    public class WebServer
    {
        public static ListDBHook<User> userDBContext;

        static void Main(string[] args)
        {
            SaneServer ws = new SaneServer("Database\\SaneDB.db", "http://+:8080/");
            ws.setHomepage("SaneWebHost.View.Home.html");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ws.addController(typeof(Controller));
            userDBContext = ws.loadModel<User>();
            TrackingList<User> users = userDBContext.getData();
            int changed = userDBContext.update();
            Console.WriteLine(changed);
            ws.run();
            stopwatch.Stop();
            Console.WriteLine("[Initialized in " + stopwatch.ElapsedMilliseconds + "ms] Webserver running!");
            Console.ReadKey();
            ws.stop();
        }
    }
}
