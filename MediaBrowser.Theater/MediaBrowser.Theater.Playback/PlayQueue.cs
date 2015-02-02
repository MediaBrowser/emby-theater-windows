using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Playback
{

    // public methods on this class are NOT thread safe
    public class PlayQueue : IPlayQueue
    {
        private readonly IList<BaseItemDto> _items;
        private readonly object _lock;

        private RepeatMode _repeatMode;
        private SortMode _sortMode;

        public event Action<RepeatMode> RepeatModeChanged;
        public event Action<SortMode> SortModeChanged;

        public PlayQueue()
        {
            _items = new List<BaseItemDto>();
            _lock = new object();
        }

        protected virtual void OnSortModeChanged(SortMode obj)
        {
            Action<SortMode> handler = SortModeChanged;
            if (handler != null) handler(obj);
        }

        protected virtual void OnRepeatModeChanged(RepeatMode obj)
        {
            Action<RepeatMode> handler = RepeatModeChanged;
            if (handler != null) handler(obj);
        }

        public RepeatMode RepeatMode
        {
            get { return _repeatMode; }
            set
            {
                lock (_lock) {
                    if (Equals(_repeatMode, value)) {
                        return;
                    }

                    _repeatMode = value;
                    OnRepeatModeChanged(value);
                }
            }
        }

        public SortMode SortMode
        {
            get { return _sortMode; }
            set
            {
                lock (_lock) {
                    if (Equals(_sortMode, value)) {
                        return;
                    }

                    _sortMode = value;
                    OnSortModeChanged(value);
                }
            }
        }

        public IPlaySequence GetPlayOrder()
        {
            return new PlaySequence(this, _lock);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null) handler(this, e);
        }

        public IEnumerator<BaseItemDto> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

        public void Add(BaseItemDto item)
        {
            lock (_items) {
                _items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _items.Count - 1));
        }

        public void AddRange(IEnumerable<BaseItemDto> items)
        {
            var itemsList = items as IList<BaseItemDto> ?? items.ToList();
            if (itemsList.Count == 0) {
                return;
            }

            lock (_items) {
                foreach (var item in itemsList) {
                    _items.Add(item);
                }
            }
            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)itemsList, _items.Count - itemsList.Count));
        }

        public void Clear()
        {
            lock (_items) {
                _items.Clear();
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(BaseItemDto item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(BaseItemDto[] array, int arrayIndex)
        {
            lock (_items) {
                _items.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(BaseItemDto item)
        {
            int index = IndexOf(item);
            if (index != -1) {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public bool IsReadOnly
        {
            get { return _items.IsReadOnly; }
        }

        public int IndexOf(BaseItemDto item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, BaseItemDto item)
        {
            lock (_items) {
                _items.Insert(index, item);
            }

            var moved = _items.Skip(index + 1).ToList();
            if (moved.Count > 0) {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, moved, index + 1, index));
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void RemoveAt(int index)
        {
            BaseItemDto item = _items[index];

            lock (_items) {
                _items.RemoveAt(index);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));

            var moved = _items.Skip(index).ToList();
            if (moved.Count > 0) {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, moved, index, index + 1));
            }
        }

        public BaseItemDto this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }
    }
}