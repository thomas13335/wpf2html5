using System;

namespace Wpf2Html5.Converter
{
    /// <summary>
    /// Arguments for the code converter error event.
    /// </summary>
    public class ConverterErrorEventArgs : EventArgs
    {
        public Exception Error { get; private set; }

        public bool Handled { get; set; }

        public ConverterErrorEventArgs(Exception ex)
        {
            Error = ex;
        }
    }

    /// <summary>
    /// Delegate for the code converter error event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ConverterErrorEventHandler(object sender, ConverterErrorEventArgs e);
}
