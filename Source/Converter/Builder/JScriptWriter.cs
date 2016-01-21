using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5.Builder
{
    /// <summary>
    /// A buffer for writing JS code.
    /// </summary>
    public class JScriptWriter
    {
        #region Private

        private StringBuilder _text = new StringBuilder();
        private int _indent = 0;
        private bool _ateol = true;
        private int _linechars = 0;

        #endregion

        #region Properties

        public string Text { get { return _text.ToString(); } }

        public bool IsCompact { get; set; }

        #endregion

        public JScriptWriter()
        { }

        #region Diagnostics

        protected bool Verbose = false;

        public override string ToString()
        {
            return _text.ToString();
        }

        protected void Trace(string format, params object[] args)
        {
            if (Verbose)
            {
                Log.Trace("jwriter: " + format, args);
            }
        }
       

        #endregion

        #region Writing Methods

        public void Write(string text)
        {
            Trace("jbuilder: write: >{0}<", text);

            if(_ateol)
            {
                _text.Append(new string(' ', _indent * 4));
                _ateol = false;
            }

            _text.Append(text);
            _linechars += text.Length;

            if (IsCompact && _linechars > 120)
            {
                _text.AppendLine(); _linechars = 0;
            }
        }

        public void Write(JScriptWriter writer)
        {
            WriteLine(writer.Text);
        }

        public void WriteLine()
        {
            if (!IsCompact)
            {
                _text.AppendLine();
                _ateol = true;
            }
        }

        public void WriteLine(string text)
        {
            Write(text);
            WriteLine();
        }

        public void Indent()
        {
            _indent++;
        }

        public void UnIndent()
        {
            _indent--;
        }

        public void EnterBlock()
        {
            WriteLine("{");
            _indent++;
        }

        public void LeaveBlock()
        {
            if (!_ateol)
            {
                WriteLine();
            }

            _indent--;
            Write("}");
        }

        public void WriteSeparator()
        {
            if(!_ateol)
            {
                WriteLine();
            }

            WriteLine();
        }

        public void Include(string filename)
        {
            using(var reader = new StreamReader(filename))
            {
                WriteLine(reader.ReadToEnd());
            }
        }

        public void WriteFunctionCall(string funame, IEnumerable<string> parameters)
        {
            WriteLine(funame + "(" + parameters.ToSeparatorList() + ");");
        }

        #endregion

        #region Static Methods

        public static string QuoteString(string s)
        {
            return "\"" + s.Replace("\"", "\\\"") + "\"";
        }

        #endregion
    }
}
