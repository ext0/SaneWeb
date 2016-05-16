using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaneWeb.Data
{
    /// <summary>
    /// Object for keeping track of changes in a List (additions, deletions, and modifications)
    /// </summary>
    /// <typeparam name="T">Model object to be stored in this object</typeparam>
    public class TrackingList<T> : IList<T> where T : Model<T>
    {
        private List<T> backing = new List<T>();
        private List<T> added = new List<T>();
        private List<T> removed = new List<T>();
        private List<T> modified = new List<T>();
        public List<T> getBacking()
        {
            return backing;
        }

        public T this[int index]
        {
            get
            {
                if (!modified.Contains(backing[index]))
                    modified.Add(backing[index]);
                return backing[index];
            }

            set
            {
                backing[index] = value;
            }
        }

        public List<T> GetAdded()
        {
            return added;
        }

        public List<T> GetRemoved()
        {
            return removed;
        }

        public List<T> GetModified()
        {
            return modified;
        }

        /// <summary>
        /// Marks an object in the list as modified, which will queue it for DB table updates in the next ListDBHook update call
        /// </summary>
        /// <param name="obj">Object to be marked as modified</param>
        public void MarkModified(T obj)
        {
            if (!modified.Contains(obj))
            {
                modified.Add(obj);
            }
        }

        public void ClearCache()
        {
            added.Clear();
            removed.Clear();
            modified.Clear();
        }

        public int Count
        {
            get
            {
                return backing.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            backing.Add(item);
            added.Add(item);
        }

        public void PreAdd(T item)
        {
            backing.Add(item);
        }

        public void Clear()
        {
            backing.Clear();
        }

        public bool Contains(T item)
        {
            return backing.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            backing.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return backing.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return backing.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            backing.Insert(index, item);
            added.Add(item);
        }

        public bool Remove(T item)
        {
            removed.Add(item);
            return backing.Remove(item);
        }

        public void RemoveAt(int index)
        {
            removed.Add(backing[index]);
            backing.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return backing.GetEnumerator();
        }
    }
}
