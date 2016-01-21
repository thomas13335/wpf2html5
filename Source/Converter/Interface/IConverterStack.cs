using System;
using System.Collections.Generic;

namespace Wpf2Html5.Converter.Interface
{
    public interface IConverterStack
    {
        void Push(object obj);

        void Pop();

        object GetCurrent(Type type);

        IEnumerable<T> GetAncestors<T>();
    }
}
