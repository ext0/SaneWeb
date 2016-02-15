﻿using SaneWeb.Resources.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaneWeb.Data
{
    public class ListDBHook<T> where T : Model<T>
    {
        private Type model;
        private String tableName;
        private SQLiteConnection dbConnection;
        private List<AttributeProperty> properties;
        private FieldInfo idField;
        private List<T> openData;
        private List<T> pastUpdate;

        public ListDBHook(SQLiteConnection dbConnection)
        {
            model = typeof(T);
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
            openData = new List<T>();
            pastUpdate = new List<T>();
        }

        public List<T> getData()
        {
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
                        object id = reader["id"];
                        if (!(id is DBNull))
                        {
                            idField.SetValue(obj, id);
                        }
                        openData.Add(obj);
                    }
                }
            }
            updateCheckData();
            return openData;
        }

        private void updateCheckData()
        {
            pastUpdate = new List<T>(openData.Select(x => Model<T>.deepClone(x)));
        }

        private List<T> getNewObjects()
        {
            List<T> type = new List<T>();
            foreach (T obj in openData)
            {
                bool flag = true;
                foreach (T exist in pastUpdate)
                {
                    if (obj.getId() == exist.getId())
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    type.Add(obj);
                }
            }
            return type;
        }

        private List<T> getUpdatedObjects()
        {
            List<T> type = new List<T>();
            foreach (T obj in openData)
            {
                foreach (T exist in pastUpdate)
                {
                    if (obj.getId() == exist.getId())
                    {
                        bool flag = false;
                        foreach (AttributeProperty property in properties)
                        {
                            Object objValue = property.propertyInfo.GetValue(obj);
                            Object existValue = property.propertyInfo.GetValue(exist);
                            if ((objValue == null) && (existValue == null)) continue;
                            if (!objValue.Equals(existValue))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                        {
                            type.Add(obj);
                        }
                    }
                }
            }
            return type;
        }

        private List<T> getRemovedObjects()
        {
            List<T> type = new List<T>();
            foreach (T exist in pastUpdate)
            {
                bool flag = false;
                foreach (T obj in openData)
                {
                    if (exist.getId() == obj.getId())
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    type.Add(exist);
                }
            }
            return type;
        }
        public int update()
        {
            int i = 0;
            List<T> newObjects = getNewObjects();
            List<T> updateObjects = getUpdatedObjects();
            List<T> removedObjects = getRemovedObjects();
            //Console.WriteLine("NEW: " + newObjects.Count);
            //Console.WriteLine("UPDATE: " + updateObjects.Count);
            //Console.WriteLine("REMOVED: " + removedObjects.Count);
            using (SQLiteTransaction transaction = dbConnection.BeginTransaction())
            {
                foreach (T obj in newObjects)
                {
                    String query = "INSERT INTO " + tableName + " (id," + String.Join(",", properties.Select((x) => (x.attribute.column))) + ") VALUES(" + obj.getId() + "," + String.Join(",", properties.Select((x) => ("\"" + x.propertyInfo.GetValue(obj)) + "\"")) + ")";
                    using (SQLiteCommand command = new SQLiteCommand(query, dbConnection))
                    {
                        i += command.ExecuteNonQuery();
                    }
                }
                /*
                foreach (T obj in updateObjects)
                {
                    String query = "UPDATE " + tableName + " SET ";
                    List<String> updates = new List<String>();
                    int id = (int)idField.GetValue(obj);
                    foreach (AttributeProperty property in properties)
                    {
                        updates.Add(property.attribute.column + "=\"" + property.propertyInfo.GetValue(obj) + "\"");
                    }
                    query += String.Join(",", updates) + " WHERE id=" + id;
                    using (SQLiteCommand command = new SQLiteCommand(query, dbConnection))
                    {
                        i += command.ExecuteNonQuery();
                    }
                }
                */
                transaction.Commit();
            }
                updateCheckData();
            return i;
        }
    }
}
