using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace BeetleX.FastHttpApi
{
    public class LRUCached
    {
        public LRUCached(StringComparer stringComparer)
        {
            if (stringComparer == null)
                stringComparer = StringComparer.Ordinal;
            mKeyCached = new ConcurrentDictionary<string, LinkedListNode<CacheItem>>(stringComparer);
        }

        public int MaxSize { get; set; } = 200000;

        public Action<object> Removed { get; set; }

        private ConcurrentDictionary<string, LinkedListNode<CacheItem>> mKeyCached;

        private LinkedList<CacheItem> mCachedItems = new LinkedList<CacheItem>();

        private void ActiveItem(LinkedListNode<CacheItem> item)
        {
            lock (mCachedItems)
            {
                if (item.List == mCachedItems)
                {
                    if (item != mCachedItems.First)
                    {
                        mCachedItems.Remove(item);
                        mCachedItems.AddFirst(item);
                    }
                }
            }
        }

        private void AddItem(LinkedListNode<CacheItem> item)
        {
            string removeKey = null;
            lock (mCachedItems)
            {
                if (mCachedItems.Count >= MaxSize)
                {
                    removeKey = mCachedItems.Last.Value.Key;
                    mCachedItems.RemoveLast();
                }
                mCachedItems.AddFirst(item);
            }
            if (removeKey != null)
                if (mKeyCached.TryRemove(removeKey, out LinkedListNode<CacheItem> del))
                {
                    Removed?.Invoke(del.Value.Value);
                }
        }

        private void RemoveItem(LinkedListNode<CacheItem> item)
        {
            lock (mCachedItems)
            {
                if (item.List == mCachedItems)
                    mCachedItems.Remove(item);
            }
        }

        public void Remove(string key)
        {
            if (mKeyCached.TryRemove(key, out LinkedListNode<CacheItem> del))
            {
                RemoveItem(del);
            }
        }

        public object ExistOrAdd(string key, object item)
        {
            LinkedListNode<CacheItem> node = new LinkedListNode<CacheItem>(new CacheItem(key, item));
            if (mKeyCached.TryAdd(key, node))
            {
                AddItem(node);
                return null;
            }
            if (mKeyCached.TryGetValue(key, out node))
                ActiveItem(node);
            return node?.Value.Value;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var obj = Get(key);
            value = (T)obj;
            return obj != null;
        }

        public object Get(string key)
        {
            if (mKeyCached.TryGetValue(key, out LinkedListNode<CacheItem> item))
            {
                ActiveItem(item);
                return item.Value.Value;
            }
            return null;
        }

        class CacheItem
        {

            public CacheItem(string key, object value)
            {
                Key = key;
                Value = value;
            }
            public string Key { get; set; }

            public object Value { get; set; }
        }

    }
}
