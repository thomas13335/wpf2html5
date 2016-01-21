using System;

namespace Wpf2Html5.Converter
{
    public class LoadSourcesEventArgs : EventArgs
    {
        public string Directory { get; set; }
    }

    public delegate void LoadSourceEventHandler(object sender, LoadSourcesEventArgs e);
}
