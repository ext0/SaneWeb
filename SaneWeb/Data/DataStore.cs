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

        /// <summary>
        /// Adds a global variable object to the datastore with the specified key
        /// </summary>
        /// <param name="key">The key to add the object under</param>
        /// <param name="obj">The data being stored</param>
        public static void AddGlobalVar(String key, Object obj)
        {
            globalVars.Add(key, obj);
        }

        /// <summary>
        /// Returns a global variable from the datastore
        /// </summary>
        /// <param name="key">They key in which the requested data is stored under</param>
        /// <returns>The requested object, if it exists.</returns>
        public static Object GetGlobalVar(String key)
        {
            return globalVars[key];
        }

        /// <summary>
        /// Gets all stored global variables
        /// </summary>
        /// <returns>All stored global variables</returns>
        public static IEnumerable<Object> GetGlobalVars()
        {
            return globalVars.Values;
        }

        /// <summary>
        /// Checks if an object is currently stored under the specified key
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>A boolean value representing whether or not the global variable store has an entry with the specified key</returns>
        public static bool VarExists(String key)
        {
            return globalVars.ContainsKey(key);
        }
        
        /// <summary>
        /// Removes a global variable from the data store
        /// </summary>
        /// <param name="key">The key to remove from the data store</param>
        public static void RemoveGlobalVar(String key)
        {
            globalVars.Remove(key);
        }
    }
}