using Newtonsoft.Json;
using SaneWeb.Data;
using SaneWeb.Resources.Attributes;
using SaneWeb.Web;
using SaneWebHost;
using SaneWebHost.Models;
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
        [Controller("~/getMessages/")]
        public static String getMessages(HttpListenerContext context, String body)
        {
            try
            {
                ListDBHook<Message> messageDB = (ListDBHook<Message>)DataStore.getGlobalVar("messageDB");
                return JsonConvert.SerializeObject(messageDB.getUnderlyingData());
            }
            catch
            {
                return "[]";
            }
        }
        [Controller("~/addMessage/")]
        public static String getMessages(HttpListenerContext context, String body, String data)
        {
            if (data.Trim().Length == 0) return "OK";
            ListDBHook<Message> messageDB = (ListDBHook<Message>)DataStore.getGlobalVar("messageDB");
            messageDB.getData(false).Add(new Message(HttpUtility.HtmlEncode(data), context.Request.RemoteEndPoint.Address.ToString()));
            messageDB.update();
            return "OK";
        }
    }
}
