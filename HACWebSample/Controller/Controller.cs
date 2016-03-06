using SaneWeb.Data;
using SaneWeb.Resources.Attributes;
using SaneWeb.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SaneWeb.Controller
{
    public static class Controller
    {
        [Controller("~/add/")]
        public static String test(HttpListenerContext context, String body, String num, String num2)
        {
            Console.WriteLine(body);
            try
            {
                int numa = int.Parse(num);
                int numb = int.Parse(num2);
                return (numa + numb).ToString();
            }
            catch
            {
                return "Invalid number syntax!";
            }
        }
    }
}
