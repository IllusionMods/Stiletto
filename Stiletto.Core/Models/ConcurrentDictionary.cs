using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Stiletto.Models
{
    public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
        private readonly IDictionary<TKey, TValue> _dictionary;
        private readonly ReaderWriterLockSlim _lock;

        public ConcurrentDictionary()
        {
            this._lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            this._dictionary = new Dictionary<TKey, TValue>();
        }

        public TValue this[TKey key] {
            get
            {
                try
                {
                    this._lock.EnterReadLock();
                    return this._dictionary[key];
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    this._lock.EnterWriteLock();
                    this._dictionary[key] = value;
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
        }

        public ICollection<TKey> Keys {
            get
            {
                try
                {
                    this._lock.EnterReadLock();
                    return this._dictionary.Keys;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                try
                {
                    this._lock.EnterReadLock();
                    return this._dictionary.Values;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        public int Count
        {
            get
            {
                try
                {
                    this._lock.EnterReadLock();
                    return this._dictionary.Count;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        public bool IsReadOnly => _dictionary.IsReadOnly;

        public void Add(TKey key, TValue value)
        {
            try
            {
                this._lock.EnterWriteLock();
                this._dictionary.Add(key, value);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            try
            {
                this._lock.EnterWriteLock();
                this._dictionary.AddItem(item);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            try
            {
                this._lock.EnterWriteLock();
                this._dictionary.Clear();
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            try
            {
                this._lock.EnterReadLock();
                return this._dictionary.Contains(item);
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            try
            {
                this._lock.EnterReadLock();
                return this._dictionary.ContainsKey(key);
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            try
            {
                this._lock.EnterReadLock();
                this._dictionary.CopyTo(array, arrayIndex);
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new ConcurrentEnumerator<KeyValuePair<TKey, TValue>>(this._dictionary, this._lock);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ConcurrentEnumerator<KeyValuePair<TKey, TValue>>(this._dictionary, this._lock);
        }

        public bool Remove(TKey key)
        {
            try
            {
                this._lock.EnterWriteLock();
                return this._dictionary.Remove(key);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            try
            {
                this._lock.EnterWriteLock();
                return this._dictionary.Remove(item);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                this._lock.EnterReadLock();
                return this._dictionary.TryGetValue(key, out value);
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        ~ConcurrentDictionary()
        {
            this._lock.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this._lock.Dispose();
        }
    }
}
