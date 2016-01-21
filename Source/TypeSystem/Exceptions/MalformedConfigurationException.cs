using System;

namespace Wpf2Html5.Exceptions
{
    /// <summary>
    /// Thrown when a configuration file was malformed.
    /// </summary>
    [Serializable]
    public class MalformedConfigurationException : Exception
    {
        /// <summary>
        /// Creates the exception.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        public MalformedConfigurationException(string msg) : base(msg) { }
    }

}
