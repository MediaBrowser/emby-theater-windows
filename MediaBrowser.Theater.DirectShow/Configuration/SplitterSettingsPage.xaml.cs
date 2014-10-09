using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.System;
using MediaBrowser.Theater.Presentation.Controls;

namespace MediaBrowser.Theater.DirectShow.Configuration
{
    /// <summary>
    /// Interaction logic for LavAudioSettings.xaml
    /// </summary>
    public partial class SplitterSettingsPage : Page
    {
        private readonly INavigationService _nav;
        private readonly ITheaterConfigurationManager _config;
        private readonly IMediaFilters _mediaFilters;
        private readonly IPresentationManager _presentation;

        public SplitterSettingsPage(INavigationService nav, ITheaterConfigurationManager config, IPresentationManager presentation, IMediaFilters mediaFilters)
        {
            _nav = nav;
            _config = config;
            _presentation = presentation;
            _mediaFilters = mediaFilters;

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += GeneralSettingsPage_Loaded;
            Unloaded += GeneralSettingsPage_Unloaded;

            SelectSubtitleMode.Options = new List<SelectListItem>
            {
                 new SelectListItem{ Text = "No Subtitles", Value="NoSubs"},
                 new SelectListItem{ Text = "Only Forced", Value="ForcedOnly"},
                 new SelectListItem{ Text = "Default", Value="Default"},
                 new SelectListItem{ Text = "Advanced", Value="Advanced"}
            };
        }

        void GeneralSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var config = _config.Configuration.InternalPlayerConfiguration;

            ChkEnableAutoForcedSubtitleStream.IsChecked = config.SplitterConfig.PGSForcedStream;
            ChkDeliverOnlyForcedSubs.IsChecked = config.SplitterConfig.PGSOnlyForced;
            TxtAudioLang.Text = config.SplitterConfig.PreferredAudioLanguages;
            TxtSubLang.Text = config.SplitterConfig.PreferredSubtitleLanguages;
            ChkShowTrayIcon.IsChecked = config.SplitterConfig.ShowTrayIcon;
            ChkUseAudioForHearingVisuallyImpaired.IsChecked = config.SplitterConfig.UseAudioForHearingVisuallyImpaired;
            TxtSubAdvanced.Text = config.SplitterConfig.AdvancedSubtitleConfig;

            SelectSubtitleMode.SelectedValue = config.SplitterConfig.SubtitleMode;            
        }

        void GeneralSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            var config = _config.Configuration.InternalPlayerConfiguration;

            config.SplitterConfig.ShowTrayIcon = ChkShowTrayIcon.IsChecked ?? false;
            config.SplitterConfig.PreferredSubtitleLanguages = TxtSubLang.Text;
            config.SplitterConfig.PreferredAudioLanguages = TxtAudioLang.Text;
            config.SplitterConfig.PGSOnlyForced = ChkDeliverOnlyForcedSubs.IsChecked ?? false;
            config.SplitterConfig.PGSForcedStream = ChkEnableAutoForcedSubtitleStream.IsChecked ?? false;
            config.SplitterConfig.UseAudioForHearingVisuallyImpaired = ChkUseAudioForHearingVisuallyImpaired.IsChecked ?? false;
            config.SplitterConfig.SubtitleMode = SelectSubtitleMode.SelectedValue;
            config.SplitterConfig.AdvancedSubtitleConfig = TxtSubAdvanced.Text;            

            _config.SaveConfiguration();
        }
    }
}
