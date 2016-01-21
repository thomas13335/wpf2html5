using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf2Html5.Style;

namespace Wpf2Html5.Converter.Framework
{
    class DockPanelConverter : PanelConverterBase<DockPanel>
    {
        private List<UIElement> _before = new List<UIElement>();
        private List<UIElement> _after = new List<UIElement>();
        private UIElement _center;
        private Orientation? _orientation;

        public override string HtmlTag
        {
            get { return "DIV"; }
        }

        protected override void WriteAttributes()
        {
            Analyse();

            AddCssClass("dockpanel-" + _orientation.ToString().ToLower());

            base.WriteAttributes();
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            Context.Push(new DockingContext());
            Context.Push(Orientation.Horizontal);

            bool empty = true;

            foreach (var control in _before)
            {
                Writer.WriteStartElement("div");
                Writer.WriteAttributeString("class", "dockitem before");
                CopyChildProperties(control);
                Context.Convert(control, Writer);
                Writer.WriteEndElement();
                empty = false;
            }

            if (null != _center)
            {
                Writer.WriteStartElement("div");
                Writer.WriteAttributeString("class", "dockitem stretch");

                Context.Convert(_center, Writer);

                Writer.WriteEndElement();
                empty = false;

            }

            foreach (var control in _after)
            {
                Writer.WriteStartElement("div");
                Writer.WriteAttributeString("class", "dockitem after");
                Context.Convert(control, Writer);
                Writer.WriteEndElement();
                empty = false;
            }

            if (_orientation.Value == Orientation.Vertical)
            {
                Context.AddSizeBinding(this, "ResizeVertical");
            }

            if (empty)
            {
                Writer.WriteString("\u00a0");
            }

            Context.Pop();
            Context.Pop();
        }

        private void CopyChildProperties(UIElement control)
        {
            var fe = control as FrameworkElement;
            if(fe.ReadLocalValue(FrameworkElement.MinWidthProperty) != DependencyProperty.UnsetValue)
            {
                var style = "min-width: " + fe.MinWidth + "px";
                Writer.WriteAttributeString("style", style);
            }
        }

        private void Analyse()
        {
            var dprop = DockPanel.DockProperty;

            foreach(var child in Control.Children.OfType<UIElement>())
            {
                var propval = child.ReadLocalValue(dprop);
                if (propval == DependencyProperty.UnsetValue)
                {
                    _center = child;
                }
                else
                {
                    var neworient = Orientation.Vertical;
                    switch ((Dock)propval)
                    {
                        case Dock.Left:
                            neworient = Orientation.Horizontal;
                            _before.Add(child);
                            break;

                        case Dock.Top:
                            _before.Add(child);
                            break;

                        case Dock.Right:
                            neworient = Orientation.Horizontal;
                            _after.Add(child);
                            break;

                        case Dock.Bottom:
                            _after.Add(child);
                            break;
                    }

                    if(_orientation.HasValue && _orientation.Value != neworient)
                    {
                        throw new Exception("dock panel orientiation problem.");
                    }

                    _orientation = neworient;
                }
            }

            if (!_orientation.HasValue) _orientation = Orientation.Vertical;
        }
    }
}
