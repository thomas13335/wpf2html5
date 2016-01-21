using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using Wpf2Html5.Builder;
using Wpf2Html5.Style;

namespace Wpf2Html5.Converter.Framework
{
    class FrameworkElementConverter<T> : UIElementConverter<T> where T : FrameworkElement
    {
        #region Private Fields

        private bool _hasmousebindings;
        private bool _haskeybindings;

        #endregion

        protected void EnableKeyBindings()
        {
            _haskeybindings = true;
        }

        #region Overrides

        protected override void WriteAttributes()
        {
            base.WriteAttributes();

            AllocateId(Writer);
            MapCommonProperties();
            MapDisplay(Style, Display);
            MapInputBindings();
        }

        protected override void ConvertChildren()
        {
            ProcessDataTemplates();

            base.ConvertChildren();
        }

        protected override string AllocateId()
        {
            if (!string.IsNullOrEmpty(Control.Name))
            {
                return SetID(Control.Name);
            }
            else
            {
                return base.AllocateId();
            }
        }

        #endregion

        #region Mapping Helpers


        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            if(MapProperty(FrameworkElement.IsEnabledProperty))
            {
                // TODO: there is no corresponding CSS style, circumvent.
            }

            if(!MapProperty(FrameworkElement.MarginProperty))
            {
                if (IsPropertyLocal(FrameworkElement.MarginProperty))
                {
                    Style.AddThicknessOptional(Control.Margin, "margin");
                }
            }

            if(!MapProperty(FrameworkElement.HorizontalAlignmentProperty))
            {
                Style.AddHorizontalAlignment(Control.HorizontalAlignment);
            }

            if (!MapProperty(FrameworkElement.VerticalAlignmentProperty))
            {
                if(!Style.AddVerticalAlignment(Control.VerticalAlignment))
                {
                    StretchHorizontally();
                }
            }

            if (!MapProperty(FrameworkElement.CursorProperty))
            {
                Style.AddCursor(Control.Cursor);
            }

            if (!MapProperty(FrameworkElement.WidthProperty))
            {
                Style.AddLength(Control.Width, "width");
            }

            if (!MapProperty(FrameworkElement.HeightProperty))
            {
                Style.AddLength(Control.Height, "height");
            }

            if(!MapProperty(FrameworkElement.MaxWidthProperty))
            {
                if (IsPropertyLocal(FrameworkElement.MaxWidthProperty))
                {
                    Style.AddLength(Control.MaxWidth, "max-width");
                }
            }

            if (!MapProperty(FrameworkElement.MinWidthProperty))
            {
                if (IsPropertyLocal(FrameworkElement.MinWidthProperty))
                {
                    Style.AddLength(Control.MinWidth, "min-width");
                }
            }

            if(!MapProperty(FrameworkElement.ToolTipProperty))
            {
                if (IsPropertyLocal(FrameworkElement.ToolTipProperty))
                {
                    Writer.WriteAttributeString("title", Control.ToolTip.ToString());
                }
            }


            MapCustomProperties();
        }

        private void MapCustomProperties()
        {
            // handle properties declared by the control class (not inherited)
            var controltype = Control.GetType();

            while (null != controltype && !controltype.FullName.StartsWith("System.Windows"))
            {
                // derivate, enumerate dependency properties of the control type
                foreach (var field in controltype.GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(f => f.DeclaringType == controltype)
                    .Where(f => f.FieldType == typeof(DependencyProperty)))
                {
                    var dp = (DependencyProperty)field.GetValue(null);
                    if (!MapProperty(dp))
                    {
                        // custom property constant value?
                        if (IsPropertyLocal(dp))
                        {
                            var value = Control.GetValue(dp);
                            if(null != value)
                            {
                                Writer.WriteAttributeString("dp." + dp.Name, value.ToString());
                            }
                        }
                    }
                }

                controltype = controltype.BaseType;
            }
        }

        protected virtual void MapInputBindings()
        {
            if (Control.InputBindings.OfType<InputBinding>().Any())
            {
                foreach (var mb in Control.InputBindings.OfType<MouseBinding>())
                {
                    Context.AddMouseBinding(this, mb);
                    _hasmousebindings = true;
                }

                foreach (var kb in Control.InputBindings.OfType<KeyBinding>())
                {
                    Context.AddKeyBinding(this, kb);
                    _haskeybindings = true;
                }

                if (_hasmousebindings)
                {
                    Writer.WriteAttributeString("onclick", "Mouse_Click(this, event);");
                }
            }

            if(_haskeybindings)
            {
                Writer.WriteAttributeString("onkeydown", "TextBox_OnKeyDown(this, event);");
                Writer.WriteAttributeString("onkeyup", "TextBox_OnKeyUp(this, event);");
            }
        }

        protected virtual void MapDisplay(StyleBuilder sb, DisplayKind defval)
        {
#if FOO
            if (_disablemapdisplay) return;

            DisplayKind kind = defval;
            var obj = Context.GetCurrent(typeof(DisplayKind));
            if (null != obj)
            {
                kind = (DisplayKind)obj;
            }

            var hstretch = false;

            /*if (null != Context.GetCurrent<DockingContext>())
            {
                var localvalue = Control.ReadLocalValue(DockPanel.DockProperty);
                if (localvalue != DependencyProperty.UnsetValue)
                {
                    var dock = (Dock)Control.GetValue(DockPanel.DockProperty);

                    switch (dock)
                    {
                        case Dock.Left:
                            //sb.Add("left", "0");
                            //sb.Add("float", "left");
                            break;

                        case Dock.Right:
                            sb.Add("position", "fixed");
                            sb.Add("right", "0");
                            // sb.Add("float", "right");
                            break;

                        case Dock.Top:
                            hstretch = true;
                            break;
                    }
                }
            }

            var orientationobj = Context.GetCurrent(typeof(Orientation));
            if (null != orientationobj)
            {
                var orientation = (Orientation)orientationobj;

                if(orientation == Orientation.Vertical && 
                    !sb.AllKeys.Contains("width") && 
                    IsStretchable)
                {
                    hstretch = true;
                }
            }

            if (hstretch)
            {
                StretchHorizontally();
                kind = DisplayKind.block;
            }*/ 

            sb.AddDisplay(kind);
#endif
        }

        /// <summary>
        /// Processes DataTemplates contain in the Resources of this element.
        /// </summary>
        protected void ProcessDataTemplates()
        {
            var rdict = Control.Resources;

            var templates = rdict.Values.OfType<DataTemplate>();
            if (templates.Any())
            {
                var dc = new DataTemplateContext(rdict, AllocateId());
                Context.Push(dc);

                // emit named resources first
                var list = templates.Select(t => new { Template = t, Priority = dc.KeyMap.ContainsKey(t) ? 0 : 1 });

                foreach (var template in list.OrderBy(x => x.Priority).Select(x => x.Template))
                {
                    Context.GenerateDataTemplate(template, Writer, null);
                }

                Context.Pop();
            }
        }

        protected void ProcessDataTemplate(DataTemplate template)
        {
            var dc = new DataTemplateContext(AllocateId());
            Context.Push(dc);

            var typename = template.DataType;

            // TraceTarget.Trace("begin single template {0} ...", this.GetHashCode());

            //Context.Convert(template, Writer);
            Context.GenerateDataTemplate(template, Writer, null);

            Context.Pop();
        }

        protected virtual void StretchHorizontally()
        {
            // Style.Add("width", "100%");
        }

        #endregion

    }
}
