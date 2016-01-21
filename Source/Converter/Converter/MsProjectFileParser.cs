using System;
using System.Xml;

namespace Wpf2Html5.Converter
{
    /// <summary>
    /// Parses a .csproj file to determine settings.
    /// </summary>
    class MsProjectFileParser
    {
        #region Private

        private XmlDocument dom;
        private XmlNamespaceManager nsmgr;
        private XmlElement root;

        #endregion

        public void Load(string projectfile)
        {
            dom = new XmlDocument();
            dom.Load(projectfile);
            nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("ms", "http://schemas.microsoft.com/developer/msbuild/2003");
            root = dom.DocumentElement;
        }

        public string GetPropertyValue(string propname, string dvalue = null)
        {
            var xsl = "ms:PropertyGroup/ms:" + propname;
            var e = root.SelectSingleNode(xsl, nsmgr);

            var value = dvalue;

            if(e is XmlElement)
            {
                value = e.InnerText;
            }
            else if(e is XmlAttribute)
            {
                value = e.Value;
            }
            else if(null != e)
            {
                throw new NotImplementedException("unsupported XML node.");
            }

            if (null == value)
            {
                throw new Exception("required project file property '" + propname + "' "
                    + "was not found and no default value was specified.");
            }

            return value;
        }
    }
}
