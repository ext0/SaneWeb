using SaneWeb.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaneWeb.Web
{
    public class SaneServerConfiguration
    {
        private String[] _prefixes;
        private String _dbPath;
        private String _resourceXml;

        public String[] Prefixes
        {
            get
            {
                return _prefixes;
            }
        }

        public String DBPath
        {
            get
            {
                return _dbPath;
            }
        }

        public String ResourceXML
        {
            get
            {
                return _resourceXml;
            }
        }

        private String _resourceNamespace = "Resources";

        public enum SaneServerPreset
        {
            DEFAULT,
            HTTPS
        }

        /// <summary>
        /// Attempts to create a SaneServerConfiguration based on a supplied preset. This method will attempt to find the ViewStructure.xml file through reflection.
        /// </summary>
        /// <param name="preset">The preset to be created around</param>
        public SaneServerConfiguration(SaneServerPreset preset, Assembly assembly, String namespaceString)
        {
            switch (preset)
            {
                case SaneServerPreset.DEFAULT:
                    _prefixes = new String[] { "http://+:80/" };
                    _dbPath = "Database\\SaneDB.db";
                    _resourceXml = Utility.fetchFromResource(assembly, namespaceString + "." + _resourceNamespace + "." + "ViewStructure.xml");
                    break;
                case SaneServerPreset.HTTPS:
                    _prefixes = new String[] { "https://+:443/" };
                    _dbPath = "Database\\SaneDB.db";
                    _resourceXml = Utility.fetchFromResource(assembly, namespaceString + "." + _resourceNamespace + "." + "ViewStructure.xml");
                    break;
            }
        }
    }
}
