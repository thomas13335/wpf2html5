using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Wpf2Html5.Converter.Framework
{
    class GridConverter : PanelConverterBase<Grid>
    {
        class GridEntry
        {
            public object Control;
        }

        private GridEntry[,] _map;

        public override string HtmlTag { get { return "table"; } }

        protected override void WriteAttributes()
        {
            AddCssClass("grid");

            int columns = Control.ColumnDefinitions.Count;
            int rows = Control.RowDefinitions.Count;

            if (0 == columns) columns = 1;
            if (0 == rows) rows = 1;

            _map = new GridEntry[rows, columns];

            foreach(var child in Control.Children.OfType<DependencyObject>())
            {
                var row = (int)child.GetValue(Grid.RowProperty);
                var col = (int)child.GetValue(Grid.ColumnProperty);

                _map[row, col] = new GridEntry { Control = child };
            }

            base.WriteAttributes();
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            var rows = _map.GetLength(0);
            var columns = _map.GetLength(1);
            for(int row = 0; row < rows; ++row)
            {
                Writer.WriteStartElement("tr");
                for (int col = 0; col < columns; ++col)
                {
                    Writer.WriteStartElement("td");
                    Writer.WriteAttributeString("class", "griditem");
                    Writer.WriteStartElement("div");
                    Writer.WriteAttributeString("height", "50%");

                    var entry = _map[row, col];
                    if(null != entry)
                    {
                        Context.Convert(entry.Control, Writer);
                    }

                    Writer.WriteEndElement();
                    Writer.WriteEndElement();
                }
                Writer.WriteEndElement();
            }
        }
    }
}
