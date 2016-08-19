using SaneWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaneWeb.Data
{
    public static class DataStore
    {
        private static Dictionary<String, Object> _globalVars = new Dictionary<String, Object>();
        private static Dictionary<Type, Object> _globalDAOs = new Dictionary<Type, Object>();

        /// <summary>
        /// Fetches a DAO object of type T from the global DAO datastore
        /// </summary>
        /// <typeparam name="T">The type of the model to fetch</typeparam>
        /// <returns>A ListDBHook object of type T</returns>
        public static ListDBHook<T> GetDAO<T>() where T : Model<T>
        {
            return (ListDBHook<T>)_globalDAOs[typeof(T)];
        }

        /// <summary>
        /// Checks whether a DAO for the type T exists in the global DAO datastore
        /// </summary>
        /// <typeparam name="T">The type of the model to search for</typeparam>
        /// <returns>Whether or not a DAO exists for this type</returns>
        public static bool DAOExists<T>() where T : Model<T>
        {
            return _globalDAOs.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Adds a DAO to the global DAO datastore
        /// </summary>
        /// <typeparam name="T">The type of model to add</typeparam>
        /// <param name="dbHandle">The ListDBHook<typeparamref name="Model"></typeparamref>/> to add to the datastore for the specified type T.</param>
        public static void AddDAO<T>(ListDBHook<T> dbHandle) where T : Model<T>
        {
            _globalDAOs.Add(typeof(T), dbHandle);
        }

        /// <summary>
        /// Adds a global variable object to the datastore with the specified key
        /// </summary>
        /// <param name="key">The key to add the object under</param>
        /// <param name="obj">The data being stored</param>
        public static void AddGlobalVar(String key, Object obj)
        {
            _globalVars.Add(key, obj);
        }

        /// <summary>
        /// Returns a global variable from the datastore
        /// </summary>
        /// <param name="key">They key in which the requested data is stored under</param>
        /// <returns>The requested object, if it exists.</returns>
        public static Object GetGlobalVar(String key)
        {
            return _globalVars[key];
        }

        /// <summary>
        /// Gets all stored global variables
        /// </summary>
        /// <returns>All stored global variables</returns>
        public static IEnumerable<Object> GetGlobalVars()
        {
            return _globalVars.Values;
        }

        /// <summary>
        /// Checks if an object is currently stored under the specified key
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>A boolean value representing whether or not the global variable store has an entry with the specified key</returns>
        public static bool VarExists(String key)
        {
            return _globalVars.ContainsKey(key);
        }

        /// <summary>
        /// Removes a global variable from the data store
        /// </summary>
        /// <param name="key">The key to remove from the data store</param>
        public static void RemoveGlobalVar(String key)
        {
            _globalVars.Remove(key);
        }
    }
}