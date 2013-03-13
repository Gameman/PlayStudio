namespace System.Collections.Generic
{
    public delegate void SynchronizedListAdded<T>(T sender);

    public class SynchronizedList<T> : IEnumerable<T>, IEnumerable
    {
        private readonly List<T> _list;
        private readonly object _lock;

        public SynchronizedList() 
        {
            _lock = new object();
            _list = new List<T>();
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public void Add(T item) 
        {
            lock (_lock) {
                _list.Add(item);
                if (OnAdded != null)
                    OnAdded(item);
            }
        }

        public bool Contains(T item)
        {
            lock (_lock) {
                return _list.Contains(item);
            }
        }

        public void Remove(T item)
        {
            lock (_lock) {
                _list.Remove(item);
            }
        }

        public object SyncRoot
        {
            get { return _lock; }
        }

        public T this[int index]
        {
            get {
                lock (_lock) {
                    return _list[index];
                }
            }
            set {
                lock (_lock) {
                    _list[index] = value;
                }
            }
        }

        public void Clear()
        {
            lock (_lock) {
                _list.Clear();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public event SynchronizedListAdded<T> OnAdded;

    }
}
