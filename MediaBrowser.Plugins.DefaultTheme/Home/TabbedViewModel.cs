using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public abstract class TabbedViewModel : BaseViewModel, IDisposable
    {
        private Timer _selectionChangeTimer;
        private readonly object _syncLock = new object();

        private readonly RangeObservableCollection<string> _sectionNames = new RangeObservableCollection<string>();

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
                var sections = Sections;

                return sections == null ? null : sections.CurrentItem as string;
            }
            set
            {
                var changed = !string.Equals(_currentSection, value);

                _currentSection = value;

                if (changed)
                {
                    OnPropertyChanged("CurrentSection");

                    ContentViewModel = GetContentViewModel(CurrentSection);
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
                    _selectionChangeTimer = new Timer(OnSelectionTimerFired, null, 400, Timeout.Infinite);
                }
                else
                {
                    _selectionChangeTimer.Change(400, Timeout.Infinite);
                }
            }
        }

        private void OnSelectionTimerFired(object state)
        {
            _dispatcher.InvokeAsync(UpdateCurrentSection);
        }

        private void UpdateCurrentSection()
        {
            CurrentSection = Sections.CurrentItem as string;
        }

        private async Task ReloadSections()
        {
            var views = await GetSectionNames();

            _sectionNames.Clear();
            _sectionNames.AddRange(views);

            Sections.MoveCurrentToPosition(0);
        }

        protected abstract Task<IEnumerable<string>> GetSectionNames();
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
}
