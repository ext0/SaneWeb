using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaneWeb.Resources.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public String tableName { get; }
        public TableAttribute(String tableName)
        {
            this.tableName = tableName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DatabaseValueAttribute : Attribute
    {
        public String column { get; }
        public int maxLength { get; }
        public DatabaseValueAttribute(String column, int maxLength)
        {
            this.column = column;
            this.maxLength = maxLength;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ControllerAttribute : Attribute
    {
        public String path;
        public ControllerAttribute(String path)
        {
            this.path = path;
        }
    }

    public class AttributeProperty
    {
        public readonly PropertyInfo propertyInfo;
        public readonly DatabaseValueAttribute attribute;
        public AttributeProperty(PropertyInfo propertyInfo, DatabaseValueAttribute attribute)
        {
            this.propertyInfo = propertyInfo;
            this.attribute = attribute;
        }
    }


    public class HttpArgument
    {
        public readonly String key;
        public readonly String value;
        public HttpArgument(String key, String value)
        {
            this.key = key;
            this.value = value;
        }
        public override string ToString()
        {
            return String.Format("{0}={1}", key, value);
        }
    }
}
