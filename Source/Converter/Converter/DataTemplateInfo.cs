using Wpf2Html5.Builder;

namespace Wpf2Html5.Converter
{
    /// <summary>
    /// Contains information about a data template.
    /// </summary>
    public class DataTemplateInfo
    {
        public object Control { get; set; }

        /// <summary>
        /// Unique identifier for the data template; name of the factory function.
        /// </summary>
        public string Key { get; set; }

        public string ContextID { get; set; }

        public string TypeName { get; set; }

        public object ResourceKey { get; set; }

        public JScriptWriter FactoryCode { get; set; }

    }
}
