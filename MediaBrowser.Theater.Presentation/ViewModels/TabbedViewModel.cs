using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public abstract class TabbedViewModel : BaseViewModel, IDisposable
    {
        private Timer _selectionChangeTimer;
        private readonly object _syncLock = new object();

        private readonly RangeObservableCollection<TabItem> _sectionNames = new RangeObservableCollection<TabItem>();

        private ListCollectionView _sections;
        public ListCollectionView Sections
        {
            get
            {
                EnsureSections();
                return _sections;
            }

            private set
            {
                _sections = value;

                OnPropertyChanged("Sections");
            }
        }

        private BaseViewModel _contentViewModel;
        public BaseViewModel ContentViewModel
        {
            get { return _contentViewModel; }

            private set
            {
                var old = _contentViewModel;

                var changed = !Equals(old, value);

                _contentViewModel = value;

                if (changed)
                {
                    OnPropertyChanged("ContentViewModel");

                    var disposable = old as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        private string _currentSection;
        public string CurrentSection
        {
            get
            {
                return _currentSection;
            }
            set
            {
                var changed = !string.Equals(_currentSection, value);

                _currentSection = value;

                if (changed)
                {
                    ContentViewModel = null;

                    OnPropertyChanged("CurrentSection");

                    ContentViewModel = GetContentViewModel(CurrentSection);
                }
            }
        }

        private string _currentSectionDisplayName;
        public string CurrentSectionDisplayName
        {
            get
            {
                return _currentSectionDisplayName;
            }
            set
            {
                var changed = !string.Equals(_currentSectionDisplayName, value);

                _currentSectionDisplayName = value;

                if (changed)
                {
                    OnPropertyChanged("CurrentSectionDisplayName");
                }
            }
        }

        private readonly Dispatcher _dispatcher;

        protected TabbedViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        private void EnsureSections()
        {
            if (_sections == null)
            {
                _sections = (ListCollectionView)CollectionViewSource.GetDefaultView(_sectionNames);

                _sections.CurrentChanged += Sections_CurrentChanged;

                ReloadSections();
            }
        }

        void Sections_CurrentChanged(object sender, EventArgs e)
        {
            lock (_syncLock)
            {
                if (_selectionChangeTimer == null)
                {
                    _selectionChangeTimer = new Timer(OnSelectionTimerFired, null, 600, Timeout.Infinite);
                }
                else
                {
                    _selectionChangeTimer.Change(600, Timeout.Infinite);
                }
            }
        }

        private void OnSelectionTimerFired(object state)
        {
            _dispatcher.InvokeAsync(UpdateCurrentSection);
        }

        private void UpdateCurrentSection()
        {
            var tab = Sections.CurrentItem as TabItem;

            CurrentSection = tab == null ? null : tab.Name;
            CurrentSectionDisplayName = tab == null ? null : tab.DisplayName;
        }

        private async Task ReloadSections()
        {
            var views = await GetSections();

            _sectionNames.Clear();
            _sectionNames.AddRange(views);

            Sections.MoveCurrentToPosition(0);
        }

        protected abstract Task<IEnumerable<TabItem>> GetSections();
        protected abstract BaseViewModel GetContentViewModel(string section);

        public void Dispose()
        {
            DisposeTimer();

            var disposable = ContentViewModel as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private void DisposeTimer()
        {
            lock (_syncLock)
            {
                if (_selectionChangeTimer != null)
                {
                    _selectionChangeTimer.Dispose();
                    _selectionChangeTimer = null;
                }
            }
        }
    }

    public class TabItem
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}
