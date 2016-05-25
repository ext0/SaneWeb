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
        public APIType type;
        public String contentType;
        public String verb
        {
            get
            {
                switch (type)
                {
                    case (APIType.GET):
                        return "GET";
                    case (APIType.POST):
                        return "POST";
                }
                throw new Exception("Unspecified APIType " + type + "!");
            }
        }
        public ControllerAttribute(String path, APIType type, String contentType)
        {
            this.path = path;
            this.type = type;
            this.contentType = contentType;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DataBoundViewAttribute : Attribute
    {
        public String path;
        public DataBoundViewAttribute(String path)
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

    public enum APIType
    {
        POST,
        GET
    }

    public enum ResponseErrorReason
    {
        INCORRECT_API_TYPE,
        INTERNAL_API_THROW,
        WEBSERVER_ERROR
    }

    public class SaneErrorEventArgs
    {
        /// <summary>
        /// Reason for the call to the event handler
        /// </summary>
        public ResponseErrorReason Reason { get; set; }

        /// <summary>
        /// Relevant exception object for the error
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Whether or not to continue back to the ResponseHandler after error handling, defaults to false
        /// </summary>
        public bool Propogate { get; set; }

        /// <summary>
        /// If propogating the error, the response to be delivered to the client (public facing)
        /// </summary>
        public String Response { get; set; }

        public SaneErrorEventArgs(ResponseErrorReason reason, Exception exception, bool propogate, String response)
        {
            this.Reason = reason;
            this.Exception = exception;
            this.Propogate = false;
            this.Response = response;
        }
    }
}
