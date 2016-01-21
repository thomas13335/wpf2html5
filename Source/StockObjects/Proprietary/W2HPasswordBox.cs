using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Wpf2Html5.StockObjects
{
    [GeneratorIgnore]
    public class W2HPasswordBox : UserControl, IW2HPasswordSource
    {
        private PasswordBox _p;

        public IW2HPasswordConsumer Consumer
        {
            get { return (IW2HPasswordConsumer)GetValue(ConsumerProperty); }
            set { SetValue(ConsumerProperty, value); }
        }

        public static readonly DependencyProperty ConsumerProperty =
            DependencyProperty.Register("Consumer", typeof(IW2HPasswordConsumer), typeof(W2HPasswordBox));

        

        public W2HPasswordBox()
        {
            _p = new PasswordBox();
            AddChild(_p);
        }


        public string GetPassword()
        {
            return _p.Password;
        }


        public void SetPassword(string password)
        {
            _p.Password = password;
        }

        public void ClearPassword()
        {
            _p.Password = null;
        }
    }
}
