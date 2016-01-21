using System;
using System.Collections.Generic;
using System.Text;
using Wpf2Html5.TypeSystem.Interface;
using Wpf2Html5.TypeSystem.Items;

namespace Wpf2Html5.TypeSystem.Collections
{
    /// <summary>
    /// Collection of type items.
    /// </summary>
    class ItemsCollection : IEnumerable<ITypeItem>, IDebugContext
    {
        #region Private

        protected Dictionary<string, ITypeItem> _items = new Dictionary<string, ITypeItem>();

        #endregion

        /// <summary>
        /// Returns an item given an ID, if present.
        /// </summary>
        /// <param name="id">The identifier to lookup.</param>
        /// <returns>Null if the item is not present.</returns>
        public ITypeItem GetItem(string id)
        {
            ITypeItem result;
            _items.TryGetValue(id, out result);
            return result;
        }

        public bool TryGetItem(string id, out ITypeItem result)
        {
            return _items.TryGetValue(id, out result);
        }

        public bool TryGetItem(Type type, out ITypeItem result)
        {
            return _items.TryGetValue(type.GetRName(), out result);
        }

        public void AddItem(ITypeItem item)
        {
            if(_items.ContainsKey(item.ID))
            {
                throw new Exception("an item '" + item.ID + "' already exists.");
            }

            _items[item.ID] = item;
        }

        public void AddItem(string id, ITypeItem item)
        {
            if (_items.ContainsKey(id))
            {
                throw new Exception("an item '" + id + "' already exists.");
            }

            _items[id] = item;
        }

        public IEnumerator<ITypeItem> GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        public string ToDebugString(int depth = 0, bool expand = false)
        {
            var prefix = new string(' ', 2 * depth);
            var sb = new StringBuilder();
            foreach (var pair in _items)
            {
                var item = pair.Value;

                if (item is Method || item is Variable || item is Property) continue;

                var head = prefix + "  " + item.CodeName;
                head = head.PadRight(55);
                sb.Append(head);
                sb.Append(" ");
                sb.Append(item.GetType().Name.PadRight(20));
                sb.Append(" ");
                sb.AppendFormat("{0}", item.IsMember ? "M" : " ");
                sb.AppendFormat("{0}", item.DoNotGenerate ? "n" : " ");
                sb.AppendFormat(" {0,-16}", item.GStatus);

                if (pair.Key != item.ID)
                {
                    sb.Append(" alias(" + item.ID + ")");
                }

                if(item.IsMember)
                {
                    sb.Append(" membertype(" + item.LType + ")");
                }

                sb.AppendLine();

                var dc = item as IDebugContext;
                if(expand && null != dc)
                {
                    sb.Append(dc.ToDebugString(depth, true));
                }
            }

            return sb.ToString();
        }

    }
}
