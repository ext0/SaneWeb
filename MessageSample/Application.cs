﻿using MessageSample.Models;
using MessageSample.Controllers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SaneWeb.Web;
using SaneWeb.Data;
using SaneWeb.Resources;

namespace MessageSample
{
    public class WebServer
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SaneServer ws = new SaneServer(
                (Utility.fetchFromResource(true, Assembly.GetExecutingAssembly(), "MessageSample.Resources.ViewStructure.xml")),
                "Database\\SaneDB.db",
                "http://+:80/");
            ws.setShowPublicErrors(true);
            Console.WriteLine("Initialized!");

            ws.addController(typeof(Controller));
            Console.WriteLine("Controller added!");

            ListDBHook<Message> userDBContext = ws.loadModel<Message>();
            DataStore.addGlobalVar("messageDB", userDBContext);
            Console.WriteLine("Model loaded!");

            TrackingList<Message> users = userDBContext.getData(false);
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
