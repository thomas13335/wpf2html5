using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wpf2Html5.Converter.Model
{
    public class ConverterReport : IDisposable
    {
        private StringBuilder _text = new StringBuilder();
        private static ThreadLocal<ConverterReport> _current = new ThreadLocal<ConverterReport>();
        private ConverterReport _previous;

        public static ConverterReport Current
        {
            get { return _current.Value; }
        }

        public string Text { get { return _text.ToString(); } }

        public ConverterReport()
        {
            _previous = _current.Value;
            _current.Value = this;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _current.Value = _previous;
        }

        public static void Trace(string format, params object[] args)
        {
            if(null != Current)
            {
                Current.Append(format, args);
            }
        }

        public static void Warning(string format, params object[] args)
        {
            if (null != Current)
            {
                Current.Append(format, args);
            }
        }

        private void Append(string format, params object[] args)
        {
            _text.AppendFormat(format, args);
            _text.AppendLine();
        }


    }
}
