using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.Session;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Theater.Interfaces.ViewModels;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class DisplayPreferencesViewModel : BaseViewModel
    {
        public DisplayPreferences DisplayPreferences { get; private set; }

        private readonly IApiClient _apiClient;
        private readonly IPresentationManager _presentation;
        private readonly ISessionManager _session;

        public ICommand SaveCommand { get; private set; }
        public ICommand IncreaseImageSizeCommand { get; private set; }
        public ICommand DecreaseImageSizeCommand { get; private set; }
        public ICommand ToggleScrollDirectionCommand { get; private set; }

        public DisplayPreferencesViewModel(DisplayPreferences displayPreferences, IApiClient apiClient, IPresentationManager presentation, ISessionManager session)
        {
            DisplayPreferences = displayPreferences;
            _apiClient = apiClient;
            _presentation = presentation;
            _session = session;

            SaveCommand = new RelayCommand(obj => Save());
            IncreaseImageSizeCommand = new RelayCommand(obj => IncreaseImageSize());
            DecreaseImageSizeCommand = new RelayCommand(obj => DecreaseImageSize());
            ToggleScrollDirectionCommand = new RelayCommand(obj => ToggleScrollDirection());
        }

        public ScrollDirection ScrollDirection
        {
            get { return DisplayPreferences.ScrollDirection; }
            set
            {
                var changed = DisplayPreferences.ScrollDirection != value;

                DisplayPreferences.ScrollDirection = value;

                if (changed)
                {
                    OnPropertyChanged("ScrollDirection");
                }
            }
        }

        public int PrimaryImageWidth
        {
            get { return DisplayPreferences.PrimaryImageWidth; }
            set
            {
                var changed = DisplayPreferences.PrimaryImageWidth != value;

                DisplayPreferences.PrimaryImageWidth = value;

                if (changed)
                {
                    OnPropertyChanged("PrimaryImageWidth");
                }
            }
        }

        public bool ShowSidebar
        {
            get { return DisplayPreferences.ShowSidebar; }
            set
            {
                var changed = DisplayPreferences.ShowSidebar != value;

                DisplayPreferences.ShowSidebar = value;

                if (changed)
                {
                    OnPropertyChanged("ShowSidebar");
                }
            }
        }

        public bool RememberSorting
        {
            get { return DisplayPreferences.RememberSorting; }
            set
            {
                var changed = DisplayPreferences.RememberSorting != value;

                DisplayPreferences.RememberSorting = value;

                if (changed)
                {
                    OnPropertyChanged("RememberSorting");
                }
            }
        }

        public string ViewType
        {
            get { return DisplayPreferences.ViewType; }
            set
            {
                var changed = !string.Equals(DisplayPreferences.ViewType, value);

                DisplayPreferences.ViewType = value;

                if (changed)
                {
                    OnPropertyChanged("ViewType");
                }
            }
        }

        public string SortBy
        {
            get { return DisplayPreferences.SortBy; }
            set
            {
                var changed = !string.Equals(DisplayPreferences.SortBy, value);

                DisplayPreferences.SortBy = value;

                if (changed)
                {
                    OnPropertyChanged("SortBy");
                }
            }
        }

        public async Task Save()
        {
            try
            {
                await _apiClient.UpdateDisplayPreferencesAsync(DisplayPreferences, _session.CurrentUser.Id, "DefaultTheme");
            }
            catch
            {
                _presentation.ShowDefaultErrorMessage();
            }
        }

        public void IncreaseImageSize()
        {
            DisplayPreferences.IncreaseImageSize();
        }

        public void DecreaseImageSize()
        {
            DisplayPreferences.DecreaseImageSize();
        }

        public void ToggleScrollDirection()
        {
            ScrollDirection = ScrollDirection == ScrollDirection.Horizontal
                                                     ? ScrollDirection.Vertical
                                                     : ScrollDirection.Horizontal;
        }
    }
}
