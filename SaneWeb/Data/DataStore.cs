using SaneWeb.LuaEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaneWeb.Data
{
    public static class DataStore
    {
        private static Dictionary<String, Object> globalVars = new Dictionary<String, Object>();

        private static Dictionary<String, List<LuaObjWrapper<Object>>> cookieValues = new Dictionary<String, List<LuaObjWrapper<Object>>>();

        public static void addCookieEntry(String cookieValue)
        {
            removeCookieEntry(cookieValue);
            cookieValues.Add(cookieValue, new List<LuaObjWrapper<Object>>());
        }

        public static void addEntryForCookie(String cookieValue, String identifier, Object obj)
        {
            if (cookieValues.ContainsKey(cookieValue))
            {
                cookieValues[cookieValue].Add(new LuaObjWrapper<Object>(identifier, obj));
            }
        }

        public static void removeCookieEntry(String cookieValue)
        {
            if (cookieValues.ContainsKey(cookieValue))
            {
                cookieValues.Remove(cookieValue);
            }
        }

        public static List<LuaObjWrapper<Object>> getCookieEntries(String cookieValue)
        {
            if (cookieValues.ContainsKey(cookieValue))
            {
                return cookieValues[cookieValue];
            }
            return null;
        }

        public static void addGlobalVar(String key, Object obj)
        {
            globalVars.Add(key, obj);
        }

        public static Object getGlobalVar(String key)
        {
            return globalVars[key];
        }

        public static IEnumerable<Object> getGlobalVars()
        {
            return globalVars.Values;
        }

        public static bool varExists(String key)
        {
            return globalVars.ContainsKey(key);
        }

        public static void removeGlobalVar(String key)
        {
            globalVars.Remove(key);
        }
    }
}