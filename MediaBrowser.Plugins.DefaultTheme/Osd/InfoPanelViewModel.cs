using MediaBrowser.Theater.Presentation.ViewModels;
using System.Collections.Generic;
using System.Linq;
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

            var nowPlaying = _transportViewModel.NowPlayingItem;
            if (nowPlaying != null && nowPlaying.People != null && nowPlaying.People.Any(i => i.HasPrimaryImage))
            {
                list.Add(new TabItem
                {
                    DisplayName = "Cast",
                    Name = "Cast"
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

        protected override object GetContentViewModel(string section)
        {
            if (string.Equals(section, "Scenes"))
            {
                var vm = _transportViewModel.CreateChaptersViewModel();

                return vm;
            }

            if (string.Equals(section, "Cast"))
            {
                var vm = _transportViewModel.CreatePeopleViewModel();

                vm.ImageWidth = 220;

                return vm;
            }

            if (string.Equals(section, "Audio"))
            {
                var vm = _transportViewModel.CreateAudioStreamsViewModel();

                return vm;
            }

            if (string.Equals(section, "Subtitles"))
            {
                var vm = _transportViewModel.CreateSubtitleStreamsViewModel();

                return vm;
            }

            return _transportViewModel;
        }

        protected override void DisposePreviousSection(object old)
        {
            if (!(old is TransportOsdViewModel))
            {
                base.DisposePreviousSection(old);
            }
        }
    }
}
