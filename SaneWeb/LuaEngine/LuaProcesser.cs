using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using DynamicLua;
using SaneWeb.Data;
using System.Reflection;
using System.Net;
using System.Diagnostics;

namespace SaneWeb.LuaEngine
{
    public class LuaObjWrapper<T>
    {
        public String identifier { get; set; }
        private T obj { get; set; }
        public LuaObjWrapper(String identifier, T obj)
        {
            this.identifier = identifier;
            this.obj = obj;
        }
        public Object getValue(String varName)
        {
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                if (property.Name.Equals(varName))
                {
                    return property.GetValue(obj);
                }
            }
            return null;
        }
    }
    public class LuaProcesser
    {
        private String processedHTMLBacking;
        private HttpListenerContext context;
        public byte[] processedHTML
        {
            get
            {
                if (!processed)
                {
                    throw new Exception("Could not successfully parse Lua from HTML!");
                }
                return Encoding.UTF8.GetBytes(processedHTMLBacking);
            }
            private set { }
        }
        private bool processed { get; set; }

        public LuaProcesser(HttpListenerContext context, byte[] data)
        {
            this.context = context;
            Dictionary<HtmlNode, String> dynamicContent = new Dictionary<HtmlNode, String>();
            processedHTMLBacking = Encoding.UTF8.GetString(data);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(processedHTMLBacking);
            if (document.DocumentNode == null)
            {
                processedHTMLBacking = document.DocumentNode.OuterHtml;
                processed = true;
                return;
            }
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//lua");
            if (nodes == null || nodes.Count == 0)
            {
                processedHTMLBacking = document.DocumentNode.OuterHtml;
                processed = true;
                return;
            }
            foreach (HtmlNode node in nodes)
            {
                dynamicContent.Add(node, executeLua(node, node.InnerText));
            }
            while (nodes.Count > 0)
            {
                HtmlNode parent = nodes[0].ParentNode;
                parent.ReplaceChild(HtmlNode.CreateNode(dynamicContent[nodes[0]].Trim()), nodes[0]);
                nodes.RemoveAt(0);
            }
            processedHTMLBacking = document.DocumentNode.OuterHtml;
            processed = true;
        }

        private String executeLua(HtmlNode currentNode, String lua)
        {
            try
            {
                String output = "";
                using (dynamic luaEngine = new DynamicLua.DynamicLua())
                {
                    luaEngine.cload = new Func<String, String>((cookie) =>
                    {
                        if (context != null)
                        {
                            Cookie ck = context.Request.Cookies[cookie];
                            if (ck != null)
                            {
                                return ck.Value;
                            }
                        }
                        return "";
                    });
                    luaEngine.eload = new Func<String, String, String, String>((cookie, name, var) =>
                    {
                        List<LuaObjWrapper<Object>> wrappers = DataStore.getCookieEntries(cookie);
                        if (wrappers == null)
                        {
                            return "";
                        }
                        foreach (LuaObjWrapper<Object> wrapper in wrappers)
                        {
                            if (wrapper.identifier.Equals(name))
                            {
                                return wrapper.getValue(var).ToString();
                            }
                        }
                        return "";
                    });
                    luaEngine.show = new Action<String>((data) =>
                    {
                        output += data;
                    });
                    luaEngine(lua);
                }
                return output;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
