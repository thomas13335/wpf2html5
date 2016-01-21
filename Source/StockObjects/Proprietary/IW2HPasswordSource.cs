using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5.StockObjects
{
    public interface IW2HPasswordSource
    {
        string GetPassword();

        void SetPassword(string password);

        void ClearPassword();
    }
}
