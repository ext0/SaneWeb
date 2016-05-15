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
        private Type model { get; set; }

        /// <summary>
        /// Name of the table this object is bound to
        /// </summary>
        private String tableName { get; set; }

        /// <summary>
        /// SQLiteConnection to the database storing this bound data
        /// </summary>
        private SQLiteConnection dbConnection { get; set; }

        /// <summary>
        /// Properties of the Model object this object is bound to
        /// </summary>
        private List<AttributeProperty> properties { get; set; }

        /// <summary>
        /// Field storing the ID for the Model object this object is bound to
        /// </summary>
        private FieldInfo idField { get; set; }

        /// <summary>
        /// TrackingList of the bound type for this object
        /// </summary>
        private TrackingList<T> openData { get; set; }

        /// <summary>
        /// Locking object for database writing
        /// </summary>
        private Object databaseLock { get; set; }

        /// <summary>
        /// Type of the model this object is bound to
        /// </summary>
        public Type declaredType { get { return model; } }

        /// <summary>
        /// Gets the current relevant underlying data bound to this object in a List
        /// </summary>
        /// <returns>The current relevant underlying data bound to this object in a List</returns>
        public List<T> getUnderlyingData()
        {
            return openData.getBacking();
        }

        /// <summary>
        /// Creates/loads a table in the relevant SQLite DB with the required structure for storing the Model
        /// </summary>
        /// <param name="dbConnection">A database connection connected to the SQLite DB this model is to be stored in</param>
        public ListDBHook(SQLiteConnection dbConnection)
        {
            model = typeof(T);
            databaseLock = new object();
            this.dbConnection = dbConnection;
            object[] attributes = model.GetCustomAttributes(typeof(TableAttribute), true);
            if (attributes.Length == 0) throw new Exception(model.Name + " is not a valid table binding type!");
            TableAttribute attribute = attributes.First() as TableAttribute;
            tableName = attribute.tableName;
            properties = new List<AttributeProperty>();
            foreach (PropertyInfo property in model.GetProperties())
            {
                DatabaseValueAttribute valueAttribute = property.GetCustomAttribute<DatabaseValueAttribute>();
                if (valueAttribute == null) continue; //user messed up
                properties.Add(new AttributeProperty(property, valueAttribute));
            }
            idField = model.BaseType.GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
            openData = new TrackingList<T>();
        }

        /// <summary>
        /// Gets a TrackingList object with the (current) bound data in the table
        /// </summary>
        /// <param name="allowCache">Determines whether or not to force a full table read for the request, deny cache usage for fetching data on the first call to this method.</param>
        /// <returns>A TrackingList object with the (current) bound data in the table</returns>
        public TrackingList<T> getData(bool allowCache)
        {
            if ((allowCache) && (openData != null)) return openData;
            openData.Clear();
            using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + tableName, dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T obj = Activator.CreateInstance<T>();
                        foreach (AttributeProperty property in properties)
                        {
                            property.propertyInfo.SetValue(obj, reader[property.attribute.column]);
                        }
                        idField.SetValue(obj, reader["id"]);
                        openData.PreAdd(obj);
                    }
                }
            }
            return openData;
        }

        /// <summary>
        /// Updates the data stored in the bound TrackingList to the database table
        /// </summary>
        /// <returns>The amount of rows affected/created by the update</returns>
        public int update()
        {
            lock (databaseLock)
            {
                int i = 0;
                List<T> newObjects = openData.getAdded();
                List<T> updateObjects = openData.getModified();
                List<T> removedObjects = openData.getRemoved();
                using (SQLiteTransaction transaction = dbConnection.BeginTransaction())
                {
                    foreach (T obj in newObjects)
                    {
                        String query = "INSERT INTO " + tableName + " (id," + String.Join(",", properties.Select((x) => (x.attribute.column))) + ") VALUES(" + obj.getId() + "," + String.Join(",", properties.Select((x) => ("@" + x.propertyInfo.Name))) + ")";
                        using (SQLiteCommand command = new SQLiteCommand(query, dbConnection))
                        {
                            for (int j = 0; j < properties.Count; j++)
                            {
                                command.Parameters.AddWithValue("@" + properties[j].propertyInfo.Name, properties[j].propertyInfo.GetValue(obj));
                            }
                            i += command.ExecuteNonQuery();
                        }
                    }
                    foreach (T obj in updateObjects)
                    {
                        String query = "UPDATE " + tableName + " SET ";
                        List<String> updates = new List<String>();
                        foreach (AttributeProperty property in properties)
                        {
                            updates.Add(property.attribute.column + "=@" + property.propertyInfo.Name);
                        }
                        query += String.Join(",", updates) + " WHERE id=" + obj.getId();
                        using (SQLiteCommand command = new SQLiteCommand(query, dbConnection))
                        {
                            for (int j = 0; j < properties.Count; j++)
                            {
                                command.Parameters.AddWithValue("@" + properties[j].propertyInfo.Name, properties[j].propertyInfo.GetValue(obj));
                            }
                            i += command.ExecuteNonQuery();
                        }
                    }
                    foreach (T obj in removedObjects)
                    {
                        String query = "DELETE FROM " + tableName + " WHERE id=" + obj.getId();
                        using (SQLiteCommand command = new SQLiteCommand(query, dbConnection))
                        {
                            i += command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                openData.clearCache();
                return i;
            }
        }
    }
}
