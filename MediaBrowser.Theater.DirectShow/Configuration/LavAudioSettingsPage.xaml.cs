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
    public partial class LavAudioSettingsPage : Page
    {
        private readonly INavigationService _nav;
        private readonly ITheaterConfigurationManager _config;
        private readonly IMediaFilters _mediaFilters;
        private readonly IPresentationManager _presentation;

        public LavAudioSettingsPage(INavigationService nav, ITheaterConfigurationManager config, IPresentationManager presentation, IMediaFilters mediaFilters)
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

            SelectMixingEncoding.Options = new List<SelectListItem>
            {
                 new SelectListItem{ Text = "None", Value="None"},
                 new SelectListItem{ Text = "Dolby", Value="Dolby"},
                 new SelectListItem{ Text = "Dolby Pro-Logic II", Value="DPLII"}
            };

            SelectMixingLayout.Options = new List<SelectListItem>
            {
                 new SelectListItem{ Text = "Stereo", Value="Stereo"},
                 new SelectListItem{ Text = "Quadraphonic", Value="Quadraphonic"},
                 new SelectListItem{ Text = "5.1", Value="FiveDotOne"},
                 new SelectListItem{ Text = "6.1", Value="SixDotOne"},
                 new SelectListItem{ Text = "7.1", Value="SevenDotOne"}
            };

            SelectMixingSetting.Options = new List<SelectListItem>
            {
                 new SelectListItem{ Text = "Untouched Stereo", Value="1"},
                 new SelectListItem{ Text = "Normalize Matrix", Value="2"},
                 new SelectListItem{ Text = "Clip Protection", Value="4"}
            };

            SelectAudioDevice.Options = new List<SelectListItem>();
            Dictionary<string, string> audioDevices = AudioConfigurationUtils.GetAudioDevices();
            foreach (string bsOption in audioDevices.Keys)
            {
                SelectAudioDevice.Options.Add(new SelectListItem { Text = bsOption, Value = audioDevices[bsOption] });
            }

            SelectSpeakerLayout.Options = new List<SelectListItem>
            {
                 new SelectListItem{ Text = "Stereo", Value="Stereo"},
                 new SelectListItem{ Text = "Quadraphonic", Value="Quad"},
                 new SelectListItem{ Text = "Surround", Value="Surround"},
                 new SelectListItem{ Text = "5.1", Value="FiveDotOneSurround"},
                 new SelectListItem{ Text = "7.1", Value="SevenDotOneSurround"}
            };
        }

        void GeneralSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var config = _config.Configuration.InternalPlayerConfiguration;

            TxtDelay.Text = config.AudioConfig.Delay.ToString();

            ChkEnableAutoSync.IsChecked = config.AudioConfig.EnableAutoSync;
            ChkConvertToStandardLayout.IsChecked = config.AudioConfig.ConvertToStandardLayout;
            ChkExpandMono.IsChecked = config.AudioConfig.ExpandMono;
            ChkExpand61.IsChecked = config.AudioConfig.Expand61;
            ChkEnableDRC.IsChecked = config.AudioConfig.EnableDRC;
            ChkEnablePCMMixing.IsChecked = config.AudioConfig.EnablePCMMixing;
            ChkShowTrayIcon.IsChecked = config.AudioConfig.ShowTrayIcon;

            SelectMixingEncoding.SelectedValue = config.AudioConfig.MixingEncoding;
            SelectMixingLayout.SelectedValue = config.AudioConfig.MixingLayout;
            SelectMixingSetting.SelectedValue = config.AudioConfig.MixingSetting.ToString();
            SelectAudioDevice.SelectedValue = string.Empty; //the default device
            foreach (var option in SelectAudioDevice.Options)
            {
                //the audio device id can change after a driver update
                if (option.Value == config.AudioConfig.AudioDevice)
                {
                    SelectAudioDevice.SelectedValue = config.AudioConfig.AudioDevice;
                    break;
                }
            }
            SelectSpeakerLayout.SelectedValue = config.AudioConfig.SpeakerLayout;

            SldDRCLevel.Value = config.AudioConfig.DRCLevel;

            foreach (var codec in Enum.GetNames(typeof(LAVAudioCodec)))
            {
                if (!codec.Equals("NB"))
                {
                    var checkbox = new CheckBox
                    {
                        Tag = codec,
                        IsChecked = config.AudioConfig.EnabledCodecs.Contains(codec, StringComparer.OrdinalIgnoreCase),
                        Margin = new Thickness(0, 0, 30, 10)
                    };

                    var textBlock = new TextBlock();
                    textBlock.SetResourceReference(TextBlock.StyleProperty, "SmallTextBlockStyle");
                    textBlock.Text = codec;

                    checkbox.Content = textBlock;

                    WrapPanelAudioCodecs.Children.Add(checkbox);
                }
            }
        }

        void GeneralSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            var config = _config.Configuration.InternalPlayerConfiguration;

            config.AudioConfig.EnableAutoSync = ChkEnableAutoSync.IsChecked ?? false;
            config.AudioConfig.ConvertToStandardLayout = ChkConvertToStandardLayout.IsChecked ?? false;
            config.AudioConfig.ExpandMono = ChkExpandMono.IsChecked ?? false;
            config.AudioConfig.Expand61 = ChkExpand61.IsChecked ?? false;
            config.AudioConfig.EnableDRC = ChkEnableDRC.IsChecked ?? false;
            config.AudioConfig.EnablePCMMixing = ChkEnablePCMMixing.IsChecked ?? false;
            config.AudioConfig.ShowTrayIcon = ChkShowTrayIcon.IsChecked ?? false;

            config.AudioConfig.MixingEncoding = SelectMixingEncoding.SelectedValue;
            config.AudioConfig.MixingLayout = SelectMixingLayout.SelectedValue;
            config.AudioConfig.MixingSetting = int.Parse(SelectMixingSetting.SelectedValue);
            config.AudioConfig.AudioDevice = SelectAudioDevice.SelectedValue;
            config.AudioConfig.SpeakerLayout = SelectSpeakerLayout.SelectedValue;

            config.AudioConfig.DRCLevel = (int)SldDRCLevel.Value;
            config.AudioConfig.EnabledCodecs.Clear();

            foreach (CheckBox codec in WrapPanelAudioCodecs.Children)
            {
                if (codec.IsChecked ?? false)
                    config.AudioConfig.EnabledCodecs.Add(codec.Tag.ToString());
            }

            config.AudioConfig.Delay = int.Parse(TxtDelay.Text);

            _config.SaveConfiguration();
        }
    }
}
