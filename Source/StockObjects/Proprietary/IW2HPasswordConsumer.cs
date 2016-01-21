using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5.StockObjects
{
    public interface IW2HPasswordConsumer
    {
        void RegisterSource(IW2HPasswordSource source);

        void PasswordStrenghtChanged(int strength);
    }
}
