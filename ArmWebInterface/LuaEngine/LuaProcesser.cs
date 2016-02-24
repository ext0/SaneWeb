using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using DynamicLua;
using SaneWeb.Data;

namespace SaneWeb.LuaEngine
{
    public class LuaProcesser
    {
        private String processedHTMLBacking;
        private Dictionary<HtmlNode, String> dynamicContent;
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

        public LuaProcesser(byte[] data)
        {
            processedHTMLBacking = Encoding.UTF8.GetString(data);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(processedHTMLBacking);
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//lua");
            if (nodes.Count == 0)
            {
                return;
            }
            this.dynamicContent = new Dictionary<HtmlNode, String>();
            foreach (HtmlNode node in nodes)
            {
                dynamicContent.Add(node, executeLua(node, node.InnerText));
            }
            while (nodes.Count > 0)
            {
                HtmlNode current = nodes[0];
                current.InnerHtml = dynamicContent[current];
                nodes.Remove(current);
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
                    luaEngine.show = new Action<String>((data) => { output += data; });
                    luaEngine.loadData = new Func<String, Object>((data) => { return DataStore.getGlobalVar(data); });
                    luaEngine(lua);
                }
                return output;
            }
            catch(Exception e)
            {
                return String.Empty;
            }
        }
    }
}
