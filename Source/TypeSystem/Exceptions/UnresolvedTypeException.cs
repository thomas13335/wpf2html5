using System;

namespace Wpf2Html5.Exceptions
{
    /// <summary>
    /// Thrown when a type was not resolved.
    /// </summary>
    [Serializable]
    public class UnresolvedTypeException : Exception
    {        
        /// <summary>
        /// Creates the exception.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        public UnresolvedTypeException(string msg) : base(msg) { }
    }
}
