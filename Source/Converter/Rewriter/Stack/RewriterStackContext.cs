using System;

namespace Wpf2Html5.Converter.Rewriter.Stack
{
    class RewriterStackContext<T> : IDisposable where T : class
    {
        private RewriterStack _stack;
        private T _entry;

        public T Entry { get { return _entry; } }

        public RewriterStackContext(RewriterStack rewriter, T obj)
        {
            _stack = rewriter;
            _entry = obj;
            _stack.Push(_entry);
        }

        public void Dispose()
        {
            _stack.Pop();
        }
    }
}
