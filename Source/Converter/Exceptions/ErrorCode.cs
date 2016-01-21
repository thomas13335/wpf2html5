using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5.Exceptions
{
    public enum ErrorCode
    {
        InvalidConverterType,
        NotSupported,
        UnresolvedTypeName,
        ConversionNotPossible,
        PreparationError,
        RewriterError,
        UnresolvedMember,
        NotEnumerable,
        ResourceNotFound
    }

    public static class ErrorCodeExtension
    {
        public static bool IsError(this ErrorCode code)
        {
            switch(code)
            {
                case ErrorCode.ConversionNotPossible:
                    return true;

                default:
                    return false;
            }
        }
    }
}
