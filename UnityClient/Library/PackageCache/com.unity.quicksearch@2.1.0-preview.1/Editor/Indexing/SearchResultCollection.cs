using System.Collections;
using System.Collections.Generic;

namespace Unity.QuickSearch
{
    class SearchResultCollection : ICollection<SearchResult>
    {
        private HashSet<SearchResult> m_Set;

        public int Count => m_Set.Count;
        public bool IsReadOnly => false;

        public SearchResultCollection()
        {
            m_Set = new HashSet<SearchResult>();
        }

        public SearchResultCollection(IEnumerable<SearchResult> inset)
        {
            m_Set = new HashSet<SearchResult>(inset);
        }

        public void Add(SearchResult item)
        {
            #if !USE_SORTED_SET
            m_Set.Add(item);
            #else // If using SortedSet eventually
            var foundSet = m_Set.GetViewBetween(item, item);
            if (foundSet.Count == 0)
                m_Set.Add(item);
            else
            {
                if (item.score < foundSet.First().score)
                {
                    m_Set.Remove(item);
                    m_Set.Add(item);
                }
            }
            #endif
        }

        public void Clear()
        {
            m_Set.Clear();
        }

        public bool Contains(SearchResult item)
        {
            return m_Set.Contains(item);
        }

        public bool TryGetValue(ref SearchResult item)
        {
            #if USE_SORTED_SET
            var foundSet = m_Set.GetViewBetween(item, item);
            if (foundSet.Count == 0)
                return false;
            var fItem = foundSet.First();
            if (fItem.score < item.score)
                item = fItem;
            return true;
            #else
            return true;
            #endif
        }

        public void CopyTo(SearchResult[] array, int arrayIndex)
        {
            m_Set.CopyTo(array, arrayIndex);
        }

        public bool Remove(SearchResult item)
        {
            return m_Set.Remove(item);
        }

        public IEnumerator<SearchResult> GetEnumerator()
        {
            return m_Set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Set.GetEnumerator();
        }
    }
}
