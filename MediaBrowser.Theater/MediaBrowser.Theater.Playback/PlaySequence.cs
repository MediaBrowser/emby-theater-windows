using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Theater.Playback
{
    class PlaySequence : IPlaySequence
    {
        private readonly Random _rnd;
        private readonly IPlayQueue _queue;
        private readonly object _lock;

        private List<int> _indices;
        private decimal _currentIndex;

        public PlaySequence(IPlayQueue queue, object synchronizationObject)
        {
            _rnd = new Random();
            _queue = queue;
            _lock = synchronizationObject;
            _currentIndex = -1;

            queue.CollectionChanged += QueueChanged;
            queue.SortModeChanged += SortModeChanged;
        }

        void QueueChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (_lock) {
                if (_queue.SortMode == SortMode.Shuffle) {
                    RecalculateIndices(e);
                }

                AdjustCurrentIndex(e);
            }
        }

        void SortModeChanged(SortMode sortMode)
        {
            if (sortMode == SortMode.Linear && Current != null) {
                _currentIndex = _queue.IndexOf(Current);
            }
        }

        public Media Current { get; private set; }

        public bool Next()
        {
            lock (_lock) {
                bool complete = !IncrementIndex();
                if (complete) {
                    Current = null;
                } else {
                    Current = GetCurrent();
                }

                return !complete;
            }
        }

        private bool IncrementIndex()
        {
            if (_queue.RepeatMode != RepeatMode.Single || _currentIndex == -1) {
                if (Math.Round(_currentIndex) != _currentIndex) {
                    _currentIndex = Math.Ceiling(_currentIndex);
                } else {
                    _currentIndex++;
                }
            }

            if (_currentIndex >= _queue.Count) {
                if (_queue.RepeatMode == RepeatMode.All && _queue.Count > 0) {
                    _currentIndex = 0;
                    return true;
                }
                
                return false;
            }

            return true;
        }

        private void RecalculateIndices(NotifyCollectionChangedEventArgs e)
        {
            var indices = _indices ?? Enumerable.Range(0, _queue.Count - 1).ToList();

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++) {
                        indices.Insert(_rnd.Next((int)Math.Floor(_currentIndex) + 1, indices.Count), e.NewStartingIndex + i);
                    }
                    
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++) {
                        var index = indices.IndexOf(e.OldStartingIndex + i);
                        indices.RemoveAt(index);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    var shift = e.NewStartingIndex - e.OldStartingIndex;
                    indices = indices.Select(i => {
                        if (i >= e.OldStartingIndex && i < e.OldStartingIndex + e.OldItems.Count) {
                            return i + shift;
                        }

                        return i;
                    }).ToList();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    indices = Shuffle(indices, 0);
                    break;
            }

            _indices = indices;
        }

        private void AdjustCurrentIndex(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    var addShift = Math.Min((_currentIndex + 1) - e.NewStartingIndex, e.NewItems.Count);
                    _currentIndex += Math.Max(addShift, 0);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var removeShift = Math.Min(_currentIndex - e.OldStartingIndex, e.OldItems.Count);

                    if (e.OldStartingIndex <= _currentIndex && e.OldStartingIndex + e.OldItems.Count > _currentIndex) {
                        _currentIndex -= Math.Max(removeShift, 0) + 0.5m;
                    }
                    else {
                        _currentIndex -= Math.Max(removeShift, 0);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _currentIndex = -1;
                    break;
            }
        }

        public bool Previous()
        {
            lock (_lock) {
                bool complete = !DecrementIndex();
                if (complete) {
                    Current = null;
                } else {
                    Current = GetCurrent();
                }

                return !complete;
            }
        }

        private bool DecrementIndex()
        {
            if (_queue.RepeatMode == RepeatMode.Single) {
                if (_currentIndex == -1 && _queue.Count > 0) {
                    _currentIndex = 0;
                }
            }
            else if (_currentIndex != -1) {
                if (Math.Round(_currentIndex) != _currentIndex) {
                    _currentIndex = Math.Floor(_currentIndex);
                }
                else {
                    _currentIndex--;
                }
            }

            if (_currentIndex == -1) {
                if (_queue.RepeatMode == RepeatMode.All && _queue.Count > 0) {
                    _currentIndex = _queue.Count - 1;
                    return true;
                }

                return false;
            }

            return true;
        }

        private Media GetCurrent()
        {
            if (_queue.SortMode == SortMode.Shuffle) {
                if (_indices == null) {
                    _indices = Shuffle(Enumerable.Range(0, _queue.Count).ToList(), 0);
                }
            }
            else {
                _indices = null;
            }

            var index = _indices != null ? _indices[(int)_currentIndex] : (int)_currentIndex;
            return _queue[index];
        }

        private List<int> Shuffle(IList<int> indices, int startIndex)
        {
            var ordered = indices.Take(startIndex);
            var shuffled = indices.Skip(startIndex).OrderBy(i => Guid.NewGuid().ToString());
            return new[] {ordered, shuffled}.SelectMany(l => l).ToList();
        }

        public void Dispose()
        {
            _queue.CollectionChanged -= QueueChanged;
            _queue.SortModeChanged -= SortModeChanged;
        }
    }
}