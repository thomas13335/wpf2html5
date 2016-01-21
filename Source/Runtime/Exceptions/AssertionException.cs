using System;

namespace Wpf2Html5.Exceptions
{
    /// <summary>
    /// An internal assertion has failed.
    /// </summary>
    [Serializable]
    public class AssertionException : Exception
    {
        /// <summary>
        /// Creates the exception.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        public AssertionException(string msg) : base(msg) { }
    }
}
