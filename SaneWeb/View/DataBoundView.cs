using System;
using System.Text;
using System.Reflection;

namespace SaneWeb.ViewProcessor
{
    public class Marker
    {
        /// <summary>
        /// The connected relevant instance for the @ marker
        /// </summary>
        /// 
        public DataBoundView instance { get; private set; }

        /// <summary>
        /// The end index of the instance's referenced closing @ tag
        /// </summary>
        private int endIndex { get; set; }

        /// <summary>
        /// The starting index of the instance's referenced opening @ tag
        /// </summary>
        public int index { get; private set; }

        /// <summary>
        /// Length of the property string
        /// </summary>
        public int length
        {
            get
            {
                return endIndex - index;
            }
        }

        /// <summary>
        /// Data stored inside of the @ tag
        /// </summary>
        public String property
        {
            get
            {
                return instance.html.Substring(index + 1, length - 1);
            }
        }

        /// <summary>
        /// Initializes a marker for the referenced DataBoundView at the specified index
        /// </summary>
        /// <param name="view">Relevant DataBoundView object</param>
        /// <param name="index">Starting index of the @ tag</param>
        public Marker(DataBoundView view, int index)
        {
            this.instance = view;
            this.index = index;
            this.endIndex = instance.html.IndexOf("@", index + 1);
            if (endIndex == -1)
            {
                throw new Exception("Invalid view syntax at index " + index + ", no closing @ tag!");
            }
        }
    }

    public class DataBoundView
    {
        /// <summary>
        /// HTML being processed, can be fetched after initialization for processed data
        /// </summary>
        public String html { get; private set; }

        /// <summary>
        /// Data object being bound to the page
        /// </summary>
        private Object boundData { get; set; }

        /// <summary>
        /// Initializes a DataBoundView with the specified HTML String and an object to bind to the HTML. Properties in the object can be inserted dynamically into the HTML using @ tags.
        /// </summary>
        /// <param name="html">HTML to be processed</param>
        /// <param name="data">Data to bind to the HTML</param>
        public DataBoundView(String html, Object data)
        {
            this.html = html;
            this.boundData = data;
            initialize();
        }

        /// <summary>
        /// Initializes a DataBoundView with the specified HTML bytes (UTF-8 expected) and an object to bind to the HTML. Properties in the object can be inserted dynamically into the HTML using @ tags.
        /// </summary>
        /// <param name="html">UTF-8 encoded bytes to be processed</param>
        /// <param name="data">Data to bind to the HTML</param>
        public DataBoundView(byte[] html, Object data)
        {
            this.html = Encoding.UTF8.GetString(html);
            this.boundData = data;
            initialize();
        }

        /// <summary>
        /// Binds the object to the html in the relevant @ tags.
        /// </summary>
        public void initialize()
        {
            StringBuilder sb = new StringBuilder(html);
            int index = -1;
            while ((index = html.IndexOf("@", Math.Max(0, index))) != -1)
            {
                if ((index != 0) && (html[index - 1] == '\\'))
                {
                    sb.Remove(index - 1, 1);
                    index += 2;
                    html = sb.ToString();
                    continue;
                }
                Marker marker = new Marker(this, index);
                String property = getPropertyFromBinding(marker.property);
                sb.Remove(marker.index, marker.length + 1);
                sb.Insert(marker.index, property);
                index = index += property.Length;
                html = sb.ToString();
            }
        }

        /// <summary>
        /// Gets the property value with the specified property name from the bound object
        /// </summary>
        /// <param name="property">Name of the property to be returned</param>
        /// <returns>The String representation of the property, if found.</returns>
        public String getPropertyFromBinding(String property)
        {
            foreach (PropertyInfo info in boundData.GetType().GetProperties())
            {
                if (info.Name.Equals(property))
                {
                    return info.GetValue(this.boundData).ToString();
                }
            }
            throw new Exception("Property \"" + property + "\" does not exist in object \"" + boundData.GetType().ToString() + "\"");
        }
    }
}
