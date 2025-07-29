using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class SortedListWithSameKeys<TItem>
    {
        SortedDictionary<long, TItem> _dict = new SortedDictionary<long, TItem>();

        public IEnumerable<TItem> List { get { return _dict.Values; } }

        public void Add(int index, TItem item)
        {
            long _index = Convert.ToInt64(index) * 1000;
            while (_dict.ContainsKey(_index))
                _index++;

            _dict[_index] = item;
        }


        public void Remove(TItem item)
        {
            long key = -1;
            foreach (var kvp in _dict)
            {
                if (kvp.Value.Equals(item))
                {
                    key = kvp.Key;
                    break;
                }
            }

            if (key != -1)
                _dict.Remove(key);

        }
    }
}
