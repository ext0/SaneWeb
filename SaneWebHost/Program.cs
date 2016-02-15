using SaneWeb.Controller;
using SaneWeb.Data;
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
            ListDBHook<User> userDBContext = ws.loadModel<User>();
            List<User> users = userDBContext.getData();
            users.Add(new User("widoreu", "password"));
            userDBContext.update();
            users[0].password = "password123";
            userDBContext.update();
            ws.run();
            Console.WriteLine("Webserver running!");
            Console.ReadKey();
            ws.stop();
        }
    }
}
