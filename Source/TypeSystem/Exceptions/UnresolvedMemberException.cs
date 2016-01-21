using System;

namespace Wpf2Html5.Exceptions
{
    /// <summary>
    /// Thrown when a type was not resolved.
    /// </summary>
    [Serializable]
    public class UnresolvedMemberException : Exception
    {
        /// <summary>
        /// Creates the exception.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        public UnresolvedMemberException(string msg) : base(msg) { }
    }
}
