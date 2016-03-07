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
using RRISD_HAC_Access;
using System.IO;
using System.IO.Compression;
using SaneWeb.Resources;

namespace HACWeb.Controllers
{
    public static class Controller
    {
        [Controller("~/login/")]
        public static String login(HttpListenerContext context, String body, String username, String password)
        {
            if (username == null || password == null)
            {
                context.Response.StatusCode = 403;
                return String.Empty;
            }
            HAC hac = new HAC();
            CookieContainer container;
            HttpWebResponse response = hac.login(username, password, out container);
            if (response == null)
            {
                context.Response.StatusCode = 500;
                return String.Empty;
            }
            if (hac.isValidLogin(response))
            {
                if (DataStore.varExists(username))
                {
                    DataStore.removeGlobalVar(username);
                }
                DataStore.addGlobalVar(username, new UserdataStoreValue(container, response, hac));
                return "SUCCESS";
            }
            context.Response.StatusCode = 403;
            return String.Empty;
        }
        [Controller("~/getStudents/")]
        public static String getStudents(HttpListenerContext context, String body, String username)
        {
            if (username == null)
            {
                context.Response.StatusCode = 403;
                return String.Empty;
            }
            if (!DataStore.varExists(username))
            {
                context.Response.StatusCode = 403;
                return String.Empty;
            }
            UserdataStoreValue value = (UserdataStoreValue)DataStore.getGlobalVar(username);
            return Utility.serializeObjectToJSON(value.hac.getStudents(value.container, value.response.ResponseUri));
        }
        [Controller("~/changeStudent/")]
        public static String changeStudent(HttpListenerContext context, String body, String username, String studentID)
        {
            if (username == null || studentID == null)
            {
                context.Response.StatusCode = 403;
                return String.Empty;
            }
            if (!DataStore.varExists(username))
            {
                context.Response.StatusCode = 403;
                return String.Empty;
            }
            UserdataStoreValue value = (UserdataStoreValue)DataStore.getGlobalVar(username);
            bool exists = value.hac.getStudents(value.container, value.response.ResponseUri).Where((x) => (x.id.Equals(studentID))).Count() == 1;
            if (exists)
            {
                value.hac.changeStudent(studentID, value.container, value.response.ResponseUri);
                return "SUCCESS";
            }
            else
            {
                context.Response.StatusCode = 500;
                return String.Empty;
            }
        }

        [Controller("~/getData/")]
        public static String getData(HttpListenerContext context, String body, String username)
        {
            if (username == null)
            {
                context.Response.StatusCode = 403;
                return String.Empty;
            }
            if (!DataStore.varExists(username))
            {
                context.Response.StatusCode = 403;
                return String.Empty;
            }
            UserdataStoreValue value = (UserdataStoreValue)DataStore.getGlobalVar(username);
            return Utility.serializeObjectToJSON(AssignmentUtils.organizeAssignments(value.hac.getAssignments(value.container, value.response.ResponseUri)));
        }
    }

    public class UserdataStoreValue
    {
        public CookieContainer container { get; set; }
        public HttpWebResponse response { get; set; }
        public HAC hac { get; set; }
        public UserdataStoreValue(CookieContainer container, HttpWebResponse response, HAC hac)
        {
            this.container = container;
            this.hac = hac;
            this.response = response;
        }
    }
}
