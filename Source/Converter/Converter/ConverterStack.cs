using System;
using System.Collections.Generic;
using System.Linq;
using Wpf2Html5.Converter.Interface;

namespace Wpf2Html5.Converter
{
    public class ConverterStack : IConverterStack
    {
        private Stack<object> _stack = new Stack<object>();

        #region Stack

        void IConverterStack.Push(object obj)
        {
            _stack.Push(obj);
        }

        void IConverterStack.Pop()
        {
            _stack.Pop();
        }

        object IConverterStack.GetCurrent(Type type)
        {
            return _stack.Where(u => type.IsAssignableFrom(u.GetType())).FirstOrDefault();
        }

        IEnumerable<T> IConverterStack.GetAncestors<T>()
        {
            return _stack.OfType<T>();
        }

        #endregion

    }
}
