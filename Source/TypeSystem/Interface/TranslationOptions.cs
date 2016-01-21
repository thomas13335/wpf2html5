
namespace Wpf2Html5.TypeSystem.Interface
{
    /// <summary>
    /// Options for the type translation procedure.
    /// </summary>
    public enum TranslateOptions
    {
        /// <summary>
        /// Default, return existing, if present.
        /// </summary>
        None = 0,
        /// <summary>
        /// Add type if no present yet.
        /// </summary>
        Add = 1,
        /// <summary>
        /// Must exist.
        /// </summary>
        MustExist = 2
    }

    static class TranslateOptionsExtensions
    {
        public static bool IsAdd(this TranslateOptions options)
        {
            return 0 != (options & TranslateOptions.Add);
        }

        public static bool IsMustExist(this TranslateOptions options)
        {
            return 0 != (options & TranslateOptions.MustExist);
        }
    }
}
