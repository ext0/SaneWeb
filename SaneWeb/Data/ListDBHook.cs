using SaneWeb.Resources.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaneWeb.Data
{
    public class ListDBHook<T> where T : Model<T>
    {
        /// <summary>
        /// Type of the model this object is bound to
        /// </summary>
        private Type Model { get; set; }

        /// <summary>
        /// Name of the table this object is bound to
        /// </summary>
        private String TableName { get; set; }

        /// <summary>
        /// SQLiteConnection to the database storing this bound data
        /// </summary>
        private SQLiteConnection DBConnection { get; set; }

        /// <summary>
        /// Properties of the Model object this object is bound to
        /// </summary>
        private List<AttributeProperty> Properties { get; set; }

        /// <summary>
        /// Field storing the ID for the Model object this object is bound to
        /// </summary>
        private FieldInfo IDField { get; set; }

        /// <summary>
        /// TrackingList of the bound type for this object
        /// </summary>
        private TrackingList<T> OpenData { get; set; }

        /// <summary>
        /// Locking object for database writing
        /// </summary>
        private Object DBLock { get; set; }

        /// <summary>
        /// Type of the model this object is bound to
        /// </summary>
        public Type DeclaredType { get { return Model; } }

        /// <summary>
        /// Gets the current relevant underlying data bound to this object in a List
        /// </summary>
        /// <returns>The current relevant underlying data bound to this object in a List</returns>
        public List<T> getUnderlyingData()
        {
            return OpenData.getBacking();
        }

        /// <summary>
        /// Creates/loads a table in the relevant SQLite DB with the required structure for storing the Model
        /// </summary>
        /// <param name="DBConnection">A database connection connected to the SQLite DB this model is to be stored in</param>
        public ListDBHook(SQLiteConnection DBConnection)
        {
            Model = typeof(T);
            DBLock = new object();
            this.DBConnection = DBConnection;
            object[] attributes = Model.GetCustomAttributes(typeof(TableAttribute), true);
            if (attributes.Length == 0) throw new Exception(Model.Name + " is not a valid table binding type!");
            TableAttribute attribute = attributes.First() as TableAttribute;
            TableName = attribute.tableName;
            Properties = new List<AttributeProperty>();
            foreach (PropertyInfo property in Model.GetProperties())
            {
                DatabaseValueAttribute valueAttribute = property.GetCustomAttribute<DatabaseValueAttribute>();
                if (valueAttribute == null) continue; //user messed up
                Properties.Add(new AttributeProperty(property, valueAttribute));
            }
            IDField = Model.BaseType.GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
            OpenData = new TrackingList<T>();
        }

        /// <summary>
        /// Gets a TrackingList object with the (current) bound data in the table
        /// </summary>
        /// <param name="allowCache">Determines whether or not to force a full table read for the request, deny cache usage for fetching data on the first call to this method.</param>
        /// <returns>A TrackingList object with the (current) bound data in the table</returns>
        public TrackingList<T> getData(bool allowCache)
        {
            if ((allowCache) && (OpenData != null)) return OpenData;
            OpenData.Clear();
            using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + TableName, DBConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T obj = Activator.CreateInstance<T>();
                        foreach (AttributeProperty property in Properties)
                        {
                            property.propertyInfo.SetValue(obj, reader[property.attribute.column]);
                        }
                        IDField.SetValue(obj, reader["id"]);
                        OpenData.PreAdd(obj);
                    }
                }
            }
            return OpenData;
        }

        /// <summary>
        /// Adds an object to the underlying bound table (with cache) and updates the list (this method really goes against the general infrastructure of this Model system, but it's just so convienent)
        /// </summary>
        /// <param name="obj">Object to be added to the bound table</param>
        /// <returns>The amount of rows affected/created by the update</returns>
        public int addAndUpdate(T obj)
        {
            OpenData.Add(obj);
            return update();
        }

        /// <summary>
        /// Updates the data stored in the bound TrackingList to the database table
        /// </summary>
        /// <returns>The amount of rows affected/created by the update</returns>
        public int update()
        {
            lock (DBLock)
            {
                int i = 0;
                List<T> newObjects = OpenData.GetAdded();
                List<T> updateObjects = OpenData.GetModified();
                List<T> removedObjects = OpenData.GetRemoved();
                using (SQLiteTransaction transaction = DBConnection.BeginTransaction())
                {
                    foreach (T obj in newObjects)
                    {
                        String query = "INSERT INTO " + TableName + " (id," + String.Join(",", Properties.Select((x) => (x.attribute.column))) + ") VALUES(" + obj.GetId() + "," + String.Join(",", Properties.Select((x) => ("@" + x.propertyInfo.Name))) + ")";
                        using (SQLiteCommand command = new SQLiteCommand(query, DBConnection))
                        {
                            for (int j = 0; j < Properties.Count; j++)
                            {
                                command.Parameters.AddWithValue("@" + Properties[j].propertyInfo.Name, Properties[j].propertyInfo.GetValue(obj));
                            }
                            i += command.ExecuteNonQuery();
                        }
                    }
                    foreach (T obj in updateObjects)
                    {
                        String query = "UPDATE " + TableName + " SET ";
                        List<String> updates = new List<String>();
                        foreach (AttributeProperty property in Properties)
                        {
                            updates.Add(property.attribute.column + "=@" + property.propertyInfo.Name);
                        }
                        query += String.Join(",", updates) + " WHERE id=" + obj.GetId();
                        using (SQLiteCommand command = new SQLiteCommand(query, DBConnection))
                        {
                            for (int j = 0; j < Properties.Count; j++)
                            {
                                command.Parameters.AddWithValue("@" + Properties[j].propertyInfo.Name, Properties[j].propertyInfo.GetValue(obj));
                            }
                            i += command.ExecuteNonQuery();
                        }
                    }
                    foreach (T obj in removedObjects)
                    {
                        String query = "DELETE FROM " + TableName + " WHERE id=" + obj.GetId();
                        using (SQLiteCommand command = new SQLiteCommand(query, DBConnection))
                        {
                            i += command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                OpenData.ClearCache();
                return i;
            }
        }
    }
}
