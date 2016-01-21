using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Wpf2Html5.StockObjects
{
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;

        public RelayCommand(Action<object> execute)
        {
            _execute = execute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void TriggerExecuteChanged()
        {
            if (null != CanExecuteChanged) CanExecuteChanged(this, new EventArgs());
        }
    }
}
