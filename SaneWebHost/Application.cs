﻿using SaneWeb.Controller;
using SaneWeb.Data;
using SaneWeb.Resources;
using SaneWeb.Web;
using SaneWebHost.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaneWebHost
{
    public class WebServer
    {
        public static ListDBHook<User> userDBContext;

        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SaneServer ws = new SaneServer(
                (Utility.fetchFromResource(true, Assembly.GetExecutingAssembly(), "SaneWebHost.Resources.ViewStructure.xml")),
                "Database\\SaneDB.db",
                "http://+:80/");
            ws.setShowPublicErrors(true);
            Console.WriteLine("Initialized!");

            ws.setHomepage("SaneWebHost.View.Home.html");

            ws.addController(typeof(Controller));
            Console.WriteLine("Controller added!");

            userDBContext = ws.loadModel<User>();
            Console.WriteLine("Model loaded!");

            TrackingList<User> users = userDBContext.getData(false);
            Console.WriteLine("Data fetched!");

            int changed = userDBContext.update();
            ws.run();
            stopwatch.Stop();
            Console.WriteLine("[Completed in " + stopwatch.ElapsedMilliseconds + "ms] Webserver running!");
            Console.ReadKey();
            ws.stop();
        }
    }
}
