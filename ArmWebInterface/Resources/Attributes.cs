using System;
using System.Collections.Generic;
using System.Linq;
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

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
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
}
