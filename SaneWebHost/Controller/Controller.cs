using SaneWeb.Resources.Attributes;
using SaneWeb.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SaneWeb.Controller
{
    public static class Controller
    {
        [Controller("~/test/")]
        public static String test(HttpListenerContext context)
        {
            return "HEY!";
        }
    }
}
