using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Osd
{
    public class InfoPanelViewModel : TabbedViewModel
    {
        private readonly TransportOsdViewModel _transportViewModel;

        public InfoPanelViewModel(TransportOsdViewModel transportViewModel)
        {
            _transportViewModel = transportViewModel;
        }

        protected override Task<IEnumerable<TabItem>> GetSections()
        {
            var list = new List<TabItem>();

            list.Add(new TabItem
            {
                DisplayName = "Info",
                Name = "Info"
            });

            if (_transportViewModel.CanSeek && _transportViewModel.SupportsChapters)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Scenes",
                    Name = "Scenes"
                });
            }

            if (_transportViewModel.CanSelectAudioTrack)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Audio",
                    Name = "Audio"
                });
            }

            if (_transportViewModel.CanSelectSubtitleTrack)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Subtitles",
                    Name = "Subtitles"
                });
            }
            
            return Task.FromResult<IEnumerable<TabItem>>(list);
        }

        protected override BaseViewModel GetContentViewModel(string section)
        {
            if (string.Equals(section, "Scenes"))
            {
                return _transportViewModel.CreateChaptersViewModel();
            }

            return _transportViewModel;
        }
    }
}
