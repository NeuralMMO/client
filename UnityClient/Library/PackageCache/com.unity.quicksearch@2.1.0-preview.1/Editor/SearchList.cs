using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Unity.QuickSearch
{
    using ItemsById = SortedDictionary<string, SearchItem>;
    using ItemsByScore = SortedDictionary<int, SortedDictionary<string, SearchItem>>;
    using ItemsByProvider = SortedDictionary<int, SortedDictionary<int, SortedDictionary<string, SearchItem>>>;

    /// <summary>
    /// A search list represents a collection of search results that is filled
    /// </summary>
    public interface ISearchList : ICollection<SearchItem>, IDisposable
    {
        /// <summary>
        /// Indicates if the search request is still running and might return more results asynchronously.
        /// </summary>
        bool pending { get; }

        /// <summary>
        /// Any valid search context that is used to track async search request. It can be null.
        /// </summary>
        SearchContext context { get; }

        /// <summary>
        /// Yields search item until the search query is finished.
        /// Nullified items can be returned while the search request is pending.
        /// </summary>
        /// <returns>List of search items. Items can be null and must be discarded</returns>
        IEnumerable<SearchItem> Fetch();

        /// <summary>
        /// Add new items to the search list.
        /// </summary>
        /// <param name="items">List of items to be added</param>
        void AddItems(IEnumerable<SearchItem> items);

        /// <summary>
        /// Insert new search items in the current list.
        /// </summary>
        /// <param name="index">Index where the items should be inserted.</param>
        /// <param name="items">Items to be inserted.</param>
        void InsertRange(int index, IEnumerable<SearchItem> items);

        /// <summary>
        /// Return a subset of items.
        /// </summary>
        /// <param name="skipCount"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        IEnumerable<SearchItem> GetRange(int skipCount, int count);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        IEnumerable<TResult> Select<TResult>(Func<SearchItem, TResult> selector);
    }

    abstract class BaseSearchList : IDisposable
    {
        private bool m_Disposed = false;

        public SearchContext context { get; private set; }

        public bool pending
        {
            get
            {
                if (context == null)
                    return false;
                return context.searchInProgress;
            }
        }

        public BaseSearchList()
        {
            context = null;
        }

        public BaseSearchList(SearchContext searchContext)
        {
            context = searchContext;
            context.asyncItemReceived += OnAsyncItemsReceived;
        }

        ~BaseSearchList()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract IEnumerable<SearchItem> Fetch();
        public abstract void AddItems(IEnumerable<SearchItem> items);

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (context != null)
                    context.asyncItemReceived -= OnAsyncItemsReceived;

                m_Disposed = true;
            }
        }

        private void OnAsyncItemsReceived(SearchContext context, IEnumerable<SearchItem> items)
        {
            AddItems(items);
        }

        public IEnumerable<TResult> Select<TResult>(Func<SearchItem, TResult> selector)
        {
            return Fetch().Where(item => item != null).Select(item => selector(item));
        }
    }

    class SortedSearchList : BaseSearchList, ISearchList
    {
        private class IdComparer : Comparer<string>
        {
            public override int Compare(string x, string y)
            {
                return String.Compare(x, y, StringComparison.Ordinal);
            }
        }

        private ItemsByProvider m_Data = new ItemsByProvider();
        private Dictionary<string, Tuple<int, int>> m_LUT = new Dictionary<string, Tuple<int, int>>();
        private bool m_TemporaryUnordered = false;
        private List<SearchItem> m_UnorderedItems = new List<SearchItem>();

        public int Count { get; private set; }
        public bool IsReadOnly => true;
        public SearchItem this[int index] => this.ElementAt(index);

        public SortedSearchList(SearchContext searchContext) : base(searchContext)
        {
            Clear();
        }

        public SortedSearchList(IEnumerable<SearchItem> items)
        {
            FromEnumerable(items);
        }

        public static implicit operator SortedSearchList(List<SearchItem> items)
        {
            return new SortedSearchList(items);
        }

        public void FromEnumerable(IEnumerable<SearchItem> items)
        {
            Clear();
            AddItems(items);
        }

        public override IEnumerable<SearchItem> Fetch()
        {
            if (context == null)
                throw new Exception("Fetch can only be used if the search list was created with a search context.");

            while (context.searchInProgress)
                yield return null;

            var it = GetEnumerator();
            while (it.MoveNext())
            {
                if (it.Current != null)
                    yield return it.Current;
            }
        }

        public override void AddItems(IEnumerable<SearchItem> items)
        {
            foreach (var item in items)
            {
                bool shouldAdd = true;
                if (m_LUT.TryGetValue(item.id, out Tuple<int, int> alreadyContainedValues))
                {
                    if (item.provider.priority >= alreadyContainedValues.Item1 &&
                        item.score >= alreadyContainedValues.Item2)
                        shouldAdd = false;

                    if (shouldAdd)
                    {
                        m_Data[alreadyContainedValues.Item1][alreadyContainedValues.Item2].Remove(item.id);
                        m_LUT.Remove(item.id);
                        --Count;
                    }
                }

                if (!shouldAdd)
                    continue;

                if (!m_Data.TryGetValue(item.provider.priority, out var itemsByScore))
                {
                    itemsByScore = new ItemsByScore();
                    m_Data.Add(item.provider.priority, itemsByScore);
                }

                if (!itemsByScore.TryGetValue(item.score, out var itemsById))
                {
                    itemsById = new ItemsById(new IdComparer());
                    itemsByScore.Add(item.score, itemsById);
                }

                itemsById.Add(item.id, item);
                m_LUT.Add(item.id, new Tuple<int, int>(item.provider.priority, item.score));
                ++Count;
            }
        }

        public void Clear()
        {
            m_Data.Clear();
            m_LUT.Clear();
            Count = 0;
            m_TemporaryUnordered = false;
            m_UnorderedItems.Clear();
        }

        public IEnumerator<SearchItem> GetEnumerator()
        {
            if (m_TemporaryUnordered)
            {
                foreach (var item in m_UnorderedItems)
                {
                    yield return item;
                }
            }

            foreach (var itemsByPriority in m_Data)
            {
                foreach (var itemsByScore in itemsByPriority.Value)
                {
                    foreach (var itemsById in itemsByScore.Value)
                    {
                        yield return itemsById.Value;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<SearchItem> GetRange(int skipCount, int count)
        {
            int skipped = 0;
            int counted = 0;
            foreach (var item in this)
            {
                if (skipped < skipCount)
                {
                    ++skipped;
                    continue;
                }

                if (counted >= count)
                    yield break;

                yield return item;
                ++counted;
            }
        }

        public void InsertRange(int index, IEnumerable<SearchItem> items)
        {
            if (!m_TemporaryUnordered)
            {
                m_TemporaryUnordered = true;
                m_UnorderedItems = this.ToList();
            }

            var tempList = items.ToList();
            m_UnorderedItems.InsertRange(index, tempList);
            Count += tempList.Count;
        }

        public void Add(SearchItem item)
        {
            AddItems(new [] {item});
        }

        public void CopyTo(SearchItem[] array, int arrayIndex)
        {
            int i = arrayIndex;
            var it = GetEnumerator();
            while (it.MoveNext())
                array[i++] = it.Current;
        }

        public bool Contains(SearchItem item)
        {
            if (m_TemporaryUnordered)
            {
                if (m_UnorderedItems.Contains(item))
                    return true;
            }

            foreach (var itemsByPriority in m_Data)
            {
                foreach (var itemsByScore in itemsByPriority.Value)
                {
                    if (itemsByScore.Value.ContainsValue(item))
                        return true;
                }
            }

            return false;
        }

        public bool Remove(SearchItem item)
        {
            bool removed = false;
            if (m_TemporaryUnordered)
                removed = m_UnorderedItems.Remove(item);

            foreach (var itemsByPriority in m_Data)
            {
                foreach (var itemsByScore in itemsByPriority.Value)
                {
                    if (itemsByScore.Value.Remove(item.id))
                        return true;
                }
            }

            return removed;
        }
    }

    class AsyncSearchList : BaseSearchList, ISearchList
    {
        private readonly List<SearchItem> m_UnorderedItems;

        public int Count => m_UnorderedItems.Count;
        public bool IsReadOnly => true;

        public AsyncSearchList(SearchContext searchContext) : base(searchContext)
        {
            m_UnorderedItems = new List<SearchItem>();
        }

        public void Add(SearchItem item)
        {
            m_UnorderedItems.Add(item);
        }

        public override void AddItems(IEnumerable<SearchItem> items)
        {
            m_UnorderedItems.AddRange(items);
        }

        public void Clear()
        {
            m_UnorderedItems.Clear();
        }

        public bool Contains(SearchItem item)
        {
            return m_UnorderedItems.Contains(item);
        }

        public void CopyTo(SearchItem[] array, int arrayIndex)
        {
            m_UnorderedItems.CopyTo(array, arrayIndex);
        }

        public override IEnumerable<SearchItem> Fetch()
        {
            if (context == null)
                throw new Exception("Fetch can only be used if the search list was created with a search context.");

            int i = 0;
            var nextCount = m_UnorderedItems.Count;
            for (; i < nextCount; ++i)
                yield return m_UnorderedItems[i];

            while (context.searchInProgress)
            {
                if (nextCount < m_UnorderedItems.Count)
                {
                    nextCount = m_UnorderedItems.Count;
                    for (; i < nextCount; ++i)
                        yield return m_UnorderedItems[i];
                }
                else
                    yield return null; // Wait for more items...
            }

            for (; i < m_UnorderedItems.Count; ++i)
                yield return m_UnorderedItems[i];
        }

        public IEnumerator<SearchItem> GetEnumerator()
        {
            return Fetch().GetEnumerator();
        }

        public void InsertRange(int index, IEnumerable<SearchItem> items)
        {
            m_UnorderedItems.InsertRange(index, items);
        }

        public bool Remove(SearchItem item)
        {
            return m_UnorderedItems.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<SearchItem> GetRange(int skipCount, int count)
        {
            return m_UnorderedItems.GetRange(skipCount, count);
        }
    }
}