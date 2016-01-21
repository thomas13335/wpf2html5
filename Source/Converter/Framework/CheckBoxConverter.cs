using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf2Html5.Converter.Interface;
using Wpf2Html5.Style;

namespace Wpf2Html5.Converter.Framework
{
    class CheckBoxConverter : FrameworkElementConverter<CheckBox>
    {
        public override string HtmlTag { get { return "INPUT"; } }

        protected override void WriteAttributes()
        {
            base.WriteAttributes();

            Writer.WriteAttributeString("type", "checkbox");

            if (MapProperty(CheckBox.IsCheckedProperty))
            {
                Writer.WriteAttributeString("onchange", "CheckBox_Changed(this);");
            }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            if (Control.Content is string)
            {
                Writer.WriteString(Control.Content.ToString());
            }
            else if (Control.Content != null)
            {
                Context.Convert(Control.Content, Writer);
            }
        }

        public override string GetSupportMethodName(SupportMethodID methodid)
        {
            switch(methodid)
            {
                case SupportMethodID.ApplyControlText:
                    return "Control_AppendText";

                default:
                    return null;
            }
        }
    }
}
