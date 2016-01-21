using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;

namespace Wpf2Html5.Converter.Framework
{
    class HyperlinkConverter : FrameworkContentElementConverter<Hyperlink>
    {
        public override string HtmlTag
        {
            get { return "A"; }
        }

        protected override void WriteAttributes()
        {
            AllocateId(Writer);

            base.WriteAttributes();

            // AGFPYOIAPG: mark this kind of hyperlinks
            AddCssClass("hyperlink");

            if (MapProperty(Hyperlink.CommandProperty))
            {
                Writer.WriteAttributeString("onclick", "Hyperlink_Click(this);");
            }

            MapProperty(Hyperlink.CommandParameterProperty);
            MapProperty(Hyperlink.NavigateUriProperty);
        }

        protected override void ConvertChildren()
        {
            foreach(var inline in Control.Inlines)
            {
                Context.Convert(inline, Writer);
            }
        }
    }
}
