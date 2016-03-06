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
        private static Dictionary<String, SQLiteConnection> openDatabases = new Dictionary<String, SQLiteConnection>();

        private static void createDatabase(String db)
        {
            Directory.CreateDirectory(db.Substring(0, db.LastIndexOf(Path.DirectorySeparatorChar)));
            SQLiteConnection.CreateFile(db);
        }

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

        public static SQLiteConnection getDatabase(String db)
        {
            return openDatabases[db];
        }

        public static bool databaseOpen(String db)
        {
            return openDatabases.ContainsKey(db);
        }

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

        private static TableAttribute getTableInfoFrom<T>()
        {
            Type type = typeof(T);
            object[] attributes = typeof(T).GetCustomAttributes(typeof(TableAttribute), true);
            if (attributes.Length == 0) throw new Exception(type.Name + " is not a valid table binding type!");
            TableAttribute attribute = attributes.First() as TableAttribute;
            return attribute;
        }

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

        public static ListDBHook<T> openTable<T>(String db) where T : Model<T>
        {
            return new ListDBHook<T>(getDatabase(db));
        }
        public static void dropTable(String db, String table)
        {
            SQLiteCommand command = new SQLiteCommand("DROP TABLE " + table, getDatabase(db));
            command.ExecuteNonQuery();
        }

        //doesn't require SQL parameters, as this is ONLY called upon loading new models with custom metadata into the database, which is never visible to public
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
    public class TableInterface<T>
    {
        public SQLiteConnection accessedDB { get; }
        public String accessedTable { get; }
        private Type binding { get; }
        public TableInterface(SQLiteConnection accessedDB, String accessedTable)
        {
            binding = typeof(T);
            this.accessedDB = accessedDB;
            this.accessedTable = accessedTable;
        }
    }
}
