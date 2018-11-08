using System.Collections.Generic;

namespace Assets.SunsetIsland.Utilities
{
    //TODO : refector into generic LRU cache
    public class LruCache<K, V> where V : IPageable<K>
    {
        private readonly LinkedList<V> _ageQueue;
        private readonly int _capacity;
        private readonly Dictionary<K, LinkedListNode<V>> m_pages;
        private int m_loadedPages;

        public LruCache(int capacity = 10)
        {
            _capacity = capacity;
            m_loadedPages = 0;
            m_pages = new Dictionary<K, LinkedListNode<V>>();
            _ageQueue = new LinkedList<V>();
        }

        public void InsertPage(V page)
        {
            LinkedListNode<V> node;
            if (m_loadedPages > _capacity)
            {
                node = _ageQueue.Last;
                DropPage(node.Value.PageId);
                node.Value = page;
            }
            else
            {
                node = new LinkedListNode<V>(page);
            }
            m_pages.Add(page.PageId, node);
            _ageQueue.AddFirst(node);
        }

        public V GetPage(K pageId)
        {
            if (!m_pages.ContainsKey(pageId))
                return default(V);
            var node = m_pages[pageId];
            var page = node.Value;
            _ageQueue.Remove(node);
            _ageQueue.AddFirst(node);
            return page;
        }

        public bool ContainsPage(K pageId)
        {
            if (m_pages.ContainsKey(pageId))
            {
                var node = m_pages[pageId];
                _ageQueue.Remove(node);
                _ageQueue.AddFirst(node);
                return true;
            }
            return false;
        }

        public void EvictCache()
        {
            m_pages.Clear();
            _ageQueue.Clear();
            m_loadedPages = 0;
        }

        public void DropPage(K pageId)
        {
            if (!m_pages.ContainsKey(pageId))
                return;
            var node = m_pages[pageId];
            _ageQueue.Remove(node);
            m_pages.Remove(pageId);
        }
    }
}