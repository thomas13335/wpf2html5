using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5.Exceptions
{
    [Serializable]
    public class ConverterException : Exception
    {
        public ErrorCode Code { get; private set; }

        public ConverterException(ErrorCode code, string msg)
            : base(msg)
        {
            Code = code;
        }
    }
}
