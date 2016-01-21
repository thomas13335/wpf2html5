using Wpf2Html5.Exceptions;

namespace Wpf2Html5
{
    /// <summary>
    /// Represents the converter runtime environment.
    /// </summary>
    public static class Runtime
    {
        public static void Assert(bool expr)
        {
            if (!expr)
            {
                throw new AssertionException("runtime assertion failed.");
            }
        }
    }
}
