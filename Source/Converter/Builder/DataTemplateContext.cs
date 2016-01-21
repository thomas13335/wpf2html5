using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Wpf2Html5.Converter
{
    class DataTemplateContext
    {
        public string ID { get; set; }

        /// <summary>Maps resource objects back to string resource keys.</summary>
        public IDictionary<object, object> KeyMap { get; set; }

        public DataTemplateContext(ResourceDictionary rdict, string id)
            : this(id)
        {
            var kdict = new Dictionary<object, object>();

            foreach (var key in rdict.Keys.OfType<string>())
            {
                var obj = rdict[key];
                kdict[obj] = key;
            }

            KeyMap = kdict;
        }

        public DataTemplateContext(string id)
        {
            ID = id;
        }
    }
}
