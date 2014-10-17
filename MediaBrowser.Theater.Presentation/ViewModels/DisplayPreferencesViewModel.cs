using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class DisplayPreferencesViewModel : BaseViewModel
    {
        public DisplayPreferences DisplayPreferences { get; private set; }

        private readonly IPresentationManager _presentation;
        private readonly ISessionManager _sessionManager;
        private readonly ITheaterConfigurationManager _configurationManager;
        private readonly UserTheaterConfiguration _userConfig;

        public ICommand SaveCommand { get; private set; }
        public ICommand IncreaseImageSizeCommand { get; private set; }
        public ICommand DecreaseImageSizeCommand { get; private set; }
        public ICommand ToggleScrollDirectionCommand { get; private set; }

        public DisplayPreferencesViewModel(DisplayPreferences displayPreferences, IPresentationManager presentation, ITheaterConfigurationManager configurationManager, ISessionManager sessionManager)
        {
            DisplayPreferences = displayPreferences;
            _presentation = presentation;
            _configurationManager = configurationManager;
            _sessionManager = sessionManager;
            _userConfig = configurationManager.GetUserTheaterConfiguration(sessionManager.LocalUserId);

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

        public SortOrder SortOrder
        {
            get { return DisplayPreferences.SortOrder; }
            set
            {
                var changed = DisplayPreferences.SortOrder != value;

                DisplayPreferences.SortOrder = value;

                if (changed)
                {
                    OnPropertyChanged("SortOrder");
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
            get { return DisplayPreferences != null && _userConfig.RememberSortOrder; }
            set
            {
                var changed = _userConfig.RememberSortOrder != value;
                _userConfig.RememberSortOrder = value;

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
                if (DisplayPreferences != null)
                {
                    DisplayPreferences.RememberSorting = false;

                    await _configurationManager.UpdateUserTheaterConfiguration(_sessionManager.LocalUserId, _userConfig);
                    await _presentation.UpdateDisplayPreferences(DisplayPreferences, CancellationToken.None);
                }    
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
