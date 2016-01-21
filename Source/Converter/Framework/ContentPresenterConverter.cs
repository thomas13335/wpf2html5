using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;

namespace Wpf2Html5.Converter.Framework
{
    class ContentPresenterConverter : FrameworkElementConverter<ContentPresenter>
    {
        public override string HtmlTag
        {
            get { return "DIV"; }
        }

        protected override void WriteAttributes()
        {
            base.WriteAttributes();

            Writer.WriteAttributeString("data-kind", "content");
        }

        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            MapProperty(ContentPresenter.ContentProperty);

            if (Control.ContentTemplate != null)
            {
                ProcessDataTemplate(Control.ContentTemplate);
                Writer.WriteAttributeString("data-template-context", ID);
            }

        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            Writer.WriteString("\u00a0");
        }
    }
}
