
namespace Wpf2Html5.TypeSystem.Interface
{
    /// <summary>
    /// Status of type generation of an item.
    /// </summary>
    public enum TypeGenerationStatus
    {
        /// <summary>
        /// No decision has been made on the generation of the item.
        /// </summary>
        initial,
        /// <summary>
        /// The item was not resolved.
        /// </summary>
        unresolved,
        /// <summary>
        /// Source code is available for the item.
        /// </summary>
        source,
        /// <summary>
        /// Items is provided by external code.
        /// </summary>
        external,
        /// <summary>
        /// The item has failed.
        /// </summary>
        failed,
        /// <summary>
        /// Convert the source of this item.
        /// </summary>
        convert,
        /// <summary>
        /// Don't convert this item.
        /// </summary>
        cloaked
    }
}
