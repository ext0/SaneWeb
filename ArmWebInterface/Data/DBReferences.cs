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
        public static bool tableExists(String db, String table)
        {
            SQLiteDataReader reader = new SQLiteCommand("PRAGMA table_info(\"" + table + "\")", getDatabase(db)).ExecuteReader();
            if (reader.Read())
            {
                return true;
            }
            return false;
        }
        public static void createTable(String db, Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(TableAttribute), true);
            if (attributes.Length == 0) throw new Exception(type.Name + " is not a valid table binding type!");
            TableAttribute attribute = attributes.First() as TableAttribute;
            SQLiteConnection dbConnection = getDatabase(db);
            String SQLString = "CREATE TABLE " + attribute.tableName + " (";
            List<String> columns = new List<String>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                DatabaseValueAttribute valueAttribute = property.GetCustomAttribute<DatabaseValueAttribute>();
                if (valueAttribute == null) continue; //user messed up
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
