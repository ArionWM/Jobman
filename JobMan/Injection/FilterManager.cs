using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class FilterManager<T> : IFilterManager<T> where T : IFilter
    {
        private readonly SortedListWithSameKeys<T> _filters = new SortedListWithSameKeys<T>();

        public void Add(T filter)
        {
            _filters.Add(filter.Index, filter);
        }


        public T[] GetFilters()
        {
            return _filters.List.ToArray();
        }

        public void Remove(T filter)
        {
            _filters.Remove(filter);
        }
    }
}
