using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5.StockObjects
{
    [GeneratorIgnore]
    public class PathOperations
    {
        public string GetContainerPath(string path)
        {
            throw new NotImplementedException();
        }

        public string CombinePath(string container, string relativepath)
        {
            throw new NotImplementedException();
        }

        public string GetFileName(string CurrentPath)
        {
            throw new NotImplementedException();
        }

        public bool IsPrefixOf(string tobetested, string ofwhat)
        {
            throw new NotImplementedException();
        }

        public string ComposeURL(string url, string path)
        {
            if(!url.EndsWith("/") && !path.StartsWith("/"))
            {
                return url + "/" + path;
            }
            else
            {
                return url + path;
            }
        }
    }
}
