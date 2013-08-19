using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.Controls;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class HomePageViewModel : BaseViewModel, IDisposable
    {
        private readonly IPresentationManager _presentationManager;
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;
        private readonly ILogger _logger;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _nav;

        private Timer _selectionChangeTimer;
        private readonly object _syncLock = new object();

        private ListCollectionView _sections;
        public ListCollectionView Sections
        {
            get { return _sections; }

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

        private readonly Dispatcher _dispatcher;

        public HomePageViewModel(IPresentationManager presentationManager, IApiClient apiClient, ISessionManager sessionManager, ILogger logger, IImageManager imageManager, INavigationService nav)
        {
            _presentationManager = presentationManager;
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _logger = logger;
            _imageManager = imageManager;
            _nav = nav;

            _dispatcher = Dispatcher.CurrentDispatcher;

            Sections = (ListCollectionView)CollectionViewSource.GetDefaultView(_sectionNames);

            ReloadSections();

            Sections.CurrentChanged += Sections_CurrentChanged;
        }

        private readonly RangeObservableCollection<string> _sectionNames = new RangeObservableCollection<string>();

        public string CurrentSection
        {
            get
            {
                var sections = Sections;

                return sections == null ? null : sections.CurrentItem as string;
            }
        }

        private async void ReloadSections()
        {
            var views = new List<string>();

            try
            {
                var itemCounts = await _apiClient.GetItemCountsAsync(_sessionManager.CurrentUser.Id);

                if (itemCounts.MovieCount > 0 || itemCounts.TrailerCount > 0)
                {
                    views.Add("movies");
                }

                if (itemCounts.SeriesCount > 0 || itemCounts.EpisodeCount > 0)
                {
                    views.Add("tv");
                }

                //if (itemCounts.SongCount > 0)
                //{
                //    views.Add("music");
                //}
                //if (itemCounts.GameCount > 0)
                //{
                //    views.Add("games");
                //}
            }
            catch (HttpException)
            {
                _presentationManager.ShowDefaultErrorMessage();
            }

            if (_presentationManager.GetApps(_sessionManager.CurrentUser).Any())
            {
                views.Add("apps");
            }

            views.Add("media collections");

            _sectionNames.Clear();
            _sectionNames.AddRange(views);

            Sections.MoveCurrentToPosition(0);
            OnPropertyChanged("CurrentSection");
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
            ContentViewModel = null;

            OnPropertyChanged("CurrentSection");

            ReloadContentViewModel();
        }

        private void ReloadContentViewModel()
        {
            var name = CurrentSection;

            if (string.Equals(name, "apps"))
            {
                ContentViewModel = new AppListViewModel(_presentationManager, _sessionManager, _logger);
            }
            else if (string.Equals(name, "media collections"))
            {
                var vm = new ItemListViewModel(GetMediaCollectionsAsync, _presentationManager, _imageManager, _apiClient, _sessionManager, _nav)
                {
                     ImageDisplayWidth = 400,
                     ImageDisplayHeightGenerator = v => 225,
                     DisplayNameGenerator = MultiItemTile.GetDisplayName
                };

                ContentViewModel = vm;
           
            }
            else if (string.Equals(name, "games"))
            {
                ContentViewModel = new GamesViewModel();
            }
        }

        private Task<ItemsResult> GetMediaCollectionsAsync(DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = new[]
                        {
                            ItemFields.PrimaryImageAspectRatio,
                            ItemFields.DateCreated,
                            ItemFields.DisplayPreferencesId
                        },

                UserId = _sessionManager.CurrentUser.Id,

                SortBy = new[] { ItemSortBy.SortName },

                SortOrder = SortOrder.Ascending
            };

            return _apiClient.GetItemsAsync(query);
        }

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
