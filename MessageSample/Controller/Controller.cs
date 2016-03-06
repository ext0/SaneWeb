using SaneWeb.Data;
using SaneWeb.Resources;
using SaneWeb.Resources.Attributes;
using SaneWeb.Web;
using MessageSample;
using MessageSample.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MessageSample.Controller
{
    public static class Controller
    {
        [Controller("~/getMessages/")]
        public static String getMessages(HttpListenerContext context, String body)
        {
            try
            {
                ListDBHook<Message> messageDB = (ListDBHook<Message>)DataStore.getGlobalVar("messageDB");
                return Utility.serializeObjectToJSON(messageDB.getUnderlyingData());
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
