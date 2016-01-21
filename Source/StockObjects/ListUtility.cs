using System.Collections.Generic;
using System.Linq;

namespace Wpf2Html5.StockObjects
{
    [GeneratorIgnore]
    public class ListUtility<T>
    {
        public ListUtility()
        { }

        public List<T> ConvertList(object o)
        {
            throw new KeyNotFoundException();
        }

        public IEnumerator<T> GetEnumerator(object o)
        {
            throw new KeyNotFoundException();
        }
    }
}
