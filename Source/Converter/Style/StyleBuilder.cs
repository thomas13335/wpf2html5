using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf2Html5.Style;

namespace Wpf2Html5.Style
{
    class StyleBuilder : NameValueCollection
    {
        #region Diagnostics

        public override string ToString()
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach(var key in this.AllKeys)
            {
                if (first) first = false; else sb.Append(" ");
                sb.AppendFormat("{0}: {1};", key, this[key]);
            }

            return sb.ToString();
        }

        #endregion

        private void AddComposed(string value, params string[] parts)
        {
            var name = parts.Where(p => null != p).ToSeparatorList("-");
            Add(name, value);
        }

        public void AddThicknessOptional(Thickness th, string prefix, string suffix = null)
        {
            if (th.IsZero()) return;

            AddThickness(th, prefix, suffix);
        }

        public void AddThickness(Thickness th, string prefix, string suffix = null)
        {

            if (th.Left == th.Right && th.Right == th.Top && th.Right == th.Bottom)
            {
                // if (0 != th.Left)
                {
                    AddComposed(th.Left + "px", prefix, suffix);
                }
            }
            else
            {
                AddComposed(th.Left + "px", prefix, "left", suffix);
                AddComposed(th.Top + "px", prefix, "top", suffix);
                AddComposed(th.Right + "px", prefix, "right", suffix);
                AddComposed(th.Bottom + "px", prefix, "bottom", suffix);
            }
        }

        public void AddCornerRadius(CornerRadius th, string prefix, string suffix = null)
        {
            // if (th.IsZero()) return;

            if (th.TopLeft == th.TopRight && th.TopRight== th.BottomLeft && th.BottomLeft == th.BottomRight)
            {
                if (0 != th.TopLeft)
                {
                    AddComposed(th.TopLeft + "px", prefix, suffix);
                }
            }
            else
            {
                // TODO:
                /*AddComposed(th.Left + "px", prefix, "left", suffix);
                AddComposed(th.Top + "px", prefix, "top", suffix);
                AddComposed(th.Right + "px", prefix, "right", suffix);
                AddComposed(th.Bottom + "px", prefix, "bottom", suffix);*/
            }
        }

        internal void AddHorizontalAlignment(HorizontalAlignment align)
        {
            switch(align)
            {
                case HorizontalAlignment.Right:
                    Add("text-align", "right");
                    break;

                case HorizontalAlignment.Left:
                    Add("text-align", "left");
                    break;

                case HorizontalAlignment.Center:
                    Add("text-align", "center");
                    break;

                case HorizontalAlignment.Stretch:
                    break;
            }
        }

        internal bool AddVerticalAlignment(VerticalAlignment align)
        {
            bool result = true;
            switch(align)
            {
                case VerticalAlignment.Bottom:
                    Add("vertical-align", "bottom");
                    break;

                case VerticalAlignment.Center:
                    Add("vertical-align", "middle");
                    // Add("height", "100%");
                    break;

                case VerticalAlignment.Top:
                    Add("vertical-align", "top");
                    break;

                case VerticalAlignment.Stretch:
                    result = false;
                    break;
            }

            return result;
        }

        public void AddDisplay(DisplayKind kind)
        {
            var text = kind.ToString().Replace("_", "-");
            Add("display", text);
        }

        public void AddBrush(Brush brush, string prefix)
        {
            if (null != brush)
            {
                string scolor;
                if (brush is SolidColorBrush)
                {
                    scolor = ToHtml((brush as SolidColorBrush).Color);
                }
                else
                {
                    Log.Warning("unable to convert brush [" + brush + "].");
                    scolor = "rgb(128,128,128)";
                }

                Add(prefix, scolor);
            }
        }

        #region Static Conversion Functions

        public static string ToHtml(Color color)
        {
            double a = color.A;
            a /= 255;

            //return string.Format("rgba({0},{1},{2},{3:0.###})", color.R, color.G, color.B, a);
            return string.Format("rgb({0},{1},{2})", color.R, color.G, color.B);
        }

        #endregion

        internal void AddCursor(Cursor cursor)
        {
            if (cursor == Cursors.Arrow)
            {
                Add("cursor", "default");
            }
            else
            {
                //TraceTarget.Trace("unsupported cursor {0}", cursor);
            }
        }

        internal void AddLength(double p, string name)
        {
            if (!double.IsInfinity(p) && !double.IsNaN(p))
            {
                Add(name, p.ToString() + "px");
            }
        }

    }
}
