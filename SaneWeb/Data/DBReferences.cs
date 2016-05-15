using SaneWeb.Resources.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaneWeb.Data
{
    public static class DBReferences
    {
        /// <summary>
        /// A dictionary containing open database connections
        /// </summary>
        private static Dictionary<String, SQLiteConnection> openDatabases = new Dictionary<String, SQLiteConnection>();

        /// <summary>
        /// Creates a SQLite database
        /// </summary>
        /// <param name="db">The path to create the database file in</param>
        private static void createDatabase(String db)
        {
            Directory.CreateDirectory(db.Substring(0, db.LastIndexOf(Path.DirectorySeparatorChar)));
            SQLiteConnection.CreateFile(db);
        }

        /// <summary>
        /// Opens a SQLite database
        /// </summary>
        /// <param name="db">The path to fetch the database file from</param>
        public static void openDatabase(String db)
        {
            if (!File.Exists(db)) createDatabase(db);
            if (!openDatabases.ContainsKey(db))
            {
                openDatabases[db] = new SQLiteConnection("Data Source=" + db + ";Version=3;");
                openDatabases[db].Open();
            }
            else
            {
                throw new Exception("Database has already been opened!");
            }
        }

        /// <summary>
        /// Gets the database storing the relevant Model
        /// </summary>
        /// <typeparam name="T">Model type to search for</typeparam>
        /// <returns>The path of the database storing the relevant Model data</returns>
        public static String findDBStoring<T>()
        {
            foreach (String db in openDatabases.Keys)
            {
                if (tableExists<T>(db))
                {
                    return db;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a SQLiteConnection object from the specified database path
        /// </summary>
        /// <param name="db">Database path to get the connection for</param>
        /// <returns>A SQLiteConnection object from the specified database path</returns>
        public static SQLiteConnection getDatabase(String db)
        {
            return openDatabases[db];
        }

        /// <summary>
        /// Checks if a database has been opened
        /// </summary>
        /// <param name="db">The path of the database being checked</param>
        /// <returns>Whether or not the database has been opened</returns>
        public static bool databaseOpen(String db)
        {
            return openDatabases.ContainsKey(db);
        }

        /// <summary>
        /// Closes an open database connection
        /// </summary>
        /// <param name="db">The path of the database file being closed</param>
        public static void closeDatabase(String db)
        {
            if (openDatabases.ContainsKey(db))
            {
                openDatabases[db].Close();
                openDatabases.Remove(db);
            }
            else
            {
                throw new Exception("Database is not in an opened state or does not exist!");
            }
        }

        /// <summary>
        /// Checks if a certain ID is unique for a specified Model table
        /// </summary>
        /// <typeparam name="T">Model (table) to be checking inside of</typeparam>
        /// <param name="db">Database path to be checking</param>
        /// <param name="id">ID to check</param>
        /// <returns>Whether or not the ID was unique</returns>
        public static bool checkIdUnique<T>(String db, int id)
        {
            TableAttribute attribute = getTableInfoFrom<T>();
            using (SQLiteCommand command = new SQLiteCommand("SELECT EXISTS(SELECT 1 FROM " + attribute.tableName + " WHERE id=" + id + " LIMIT 1)", getDatabase(db)))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    return (Int64)reader[0] == 0;
                }
            }
        }

        /// <summary>
        /// Gets the TableAttribute object for the Model specified
        /// </summary>
        /// <typeparam name="T">The Model type to check</typeparam>
        /// <returns>The TableAttribute object for the Model specified</returns>
        private static TableAttribute getTableInfoFrom<T>()
        {
            Type type = typeof(T);
            object[] attributes = typeof(T).GetCustomAttributes(typeof(TableAttribute), true);
            if (attributes.Length == 0) throw new Exception(type.Name + " is not a valid table binding type!");
            TableAttribute attribute = attributes.First() as TableAttribute;
            return attribute;
        }

        /// <summary>
        /// Checks whether or not a table already exists for the specified Model
        /// </summary>
        /// <typeparam name="T">The Model type to check</typeparam>
        /// <param name="db">Database path to be checking</param>
        /// <returns>Whether or not a table already exists for the specified Model</returns>
        public static bool tableExists<T>(String db)
        {
            TableAttribute attribute = getTableInfoFrom<T>();
            using (SQLiteCommand command = new SQLiteCommand("PRAGMA table_info(" + attribute.tableName + ")", getDatabase(db)))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Reads a database table and binds a ListDBHook object to the table specified by the Model
        /// </summary>
        /// <typeparam name="T">Model (table) to open</typeparam>
        /// <param name="db">Database path to be opening</param>
        /// <returns></returns>
        public static ListDBHook<T> openTable<T>(String db) where T : Model<T>
        {
            return new ListDBHook<T>(getDatabase(db));
        }

        /// <summary>
        /// Removes a table from the relevant SQLite DB by its name
        /// </summary>
        /// <param name="db">Database to be removing from</param>
        /// <param name="table">Table name to remove</param>
        public static void dropTable(String db, String table)
        {
            SQLiteCommand command = new SQLiteCommand("DROP TABLE " + table, getDatabase(db));
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Creates a fresh empty table using the specified Model
        /// </summary>
        /// <typeparam name="T">Model type to be creating a table for</typeparam>
        /// <param name="db">Database path to create the Model table in</param>
        public static void createTable<T>(String db) where T : Model<T>
        {
            TableAttribute attribute = getTableInfoFrom<T>();
            SQLiteConnection dbConnection = getDatabase(db);
            String SQLString = "CREATE TABLE " + attribute.tableName + " (";
            List<String> columns = new List<String>();
            columns.Add(" id INT");
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                DatabaseValueAttribute valueAttribute = property.GetCustomAttribute<DatabaseValueAttribute>();
                if (valueAttribute == null) continue;
                columns.Add(valueAttribute.column + " VARCHAR(" + valueAttribute.maxLength + ")");
            }
            SQLString += String.Join(",", columns) + ")";
            new SQLiteCommand(SQLString, dbConnection).ExecuteNonQuery();
        }
    }
}
