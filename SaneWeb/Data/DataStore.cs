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

        public static void addGlobalVar(String key, Object obj)
        {
            globalVars.Add(key, obj);
        }

        public static Object getGlobalVar(String key)
        {
            return globalVars[key];
        }
        
        public static bool varExists(String key)
        {
            return globalVars.ContainsKey(key);
        }
    }
}
