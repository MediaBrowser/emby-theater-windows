using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ItemInfoDetailsViewModel
        : BaseViewModel
    {
        private readonly BaseItemDto _item;
        private Func<BaseItemDto, string> _displayNameGenerator;

        public ItemInfoDetailsViewModel(BaseItemDto item)
        {
            if (_item == null) {
                throw new ArgumentNullException("item");
            }

            _item = item;
        }

        public string DisplayName 
        {
            get { return DisplayNameGenerator(_item); }
        }

        public Func<BaseItemDto, string> DisplayNameGenerator
        {
            get { return _displayNameGenerator; }
            set
            {
                if (Equals(value, _displayNameGenerator)) {
                    return;
                }
                _displayNameGenerator = value;
                OnPropertyChanged();
                OnPropertyChanged("DisplayName");
            }
        }

        public string Overview 
        {
            get { return _item.Overview; }
        }

        public string Date
        {
            get
            {
                if (_item.PremiereDate.HasValue && _item.IsType("episode")) {
                    return _item.PremiereDate.Value.ToShortDateString();
                }

                if (_item.ProductionYear.HasValue) {
                    if (_item.EndDate.HasValue && _item.EndDate.Value.Year != _item.ProductionYear) {
                        return "MediaBrowser.Theater.DefaultTheme:Strings:ItemInfo_DateRange".LocalizeFormat(_item.ProductionYear.Value, _item.EndDate.Value.Year);
                    }

                    if (_item.Status.HasValue && _item.Status.Value == SeriesStatus.Continuing) {
                        return "MediaBrowser.Theater.DefaultTheme:Strings:ItemInfo_DateRangeContinuing".LocalizeFormat(_item.ProductionYear.Value);
                    }

                    return _item.ProductionYear.Value.ToString(CultureInfo.CurrentUICulture);
                }

                return null;
            }
        }

        public string AudioChannelLayout
        {
            get
            {
                if (_item.MediaStreams != null) {
                    var stream = _item.MediaStreams
                                      .OrderBy(i => i.Index)
                                      .FirstOrDefault(i => i.Type == MediaStreamType.Audio);

                    if (stream != null) {
                        return stream.ChannelLayout;
                    }
                }

                return null;
            }
        }

        public string[] Subtitles
        {
            get
            {
                if (_item.MediaStreams != null) {
                    return _item.MediaStreams.Where(i => i.Type == MediaStreamType.Subtitle && !string.IsNullOrEmpty(i.Language))
                                .Select(i => i.Language)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToArray();
                }

                return new string[] { };
            }
        }

        public string Resolution {
            get
            {
                if (_item.MediaStreams != null) {
                    var stream = _item.MediaStreams
                                      .OrderBy(i => i.Index)
                                      .FirstOrDefault(i => i.Type == MediaStreamType.Video);

                    if (stream != null) {
                        if (stream.Width.HasValue && stream.Height.HasValue) {
                            if (IsCloseTo(stream.Width.Value, 1920)) {
                                return "1080p";
                            }
                            if (IsCloseTo(stream.Width.Value, 1280)) {
                                return "720p";
                            }
                            if (IsCloseTo(stream.Width.Value, 720)) {
                                return "480p";
                            }
                            return stream.Width + "×" + stream.Height;
                        }
                    }
                }

                return null;
            }
        }

        private bool IsCloseTo(int x, int y)
        {
            return Math.Abs(x - y) <= 20;
        }

        public string SeriesAirTime
        {
            get
            {
                if (!_item.IsType("series")) {
                    return null;
                }

                string formatStringKey = "MediaBrowser.Theater.DefaultTheme:Strings:SeriesAirTime_";
                var formatParameters = new List<string>();

                if ((_item.Status ?? SeriesStatus.Continuing) == SeriesStatus.Continuing) {
                    formatStringKey += "Airing";
                } else {
                    formatStringKey += "Aired";
                }

                if (_item.AirDays != null && _item.AirDays.Count > 0) {
                    formatStringKey += "Days";
                    formatParameters.Add(_item.AirDays.Select(i => i.Localize()).ToLocalizedList());
                }

                if (!string.IsNullOrEmpty(_item.AirTime)) {
                    formatStringKey += "Time";
                    formatParameters.Add(_item.AirTime); //todo convert series air time to localized time
                }

                var studio = _item.Studios.FirstOrDefault();
                if (studio != null) {
                    formatStringKey += "Network";
                    formatParameters.Add(studio.Name);
                }

                return formatStringKey.LocalizeFormat(formatParameters);
            }
        }

        public string Runtime { get; set; }
        public string OfficialRating { get; set; }
        public float CommunityRating { get; set; }
        public bool HasCommunityRating { get; set; }
        public float CriticRating { get; set; }
        public bool HasCriticRating { get; set; }
        public bool HasPositiveCriticRating { get; set; }
        public bool HasNegativeCriticRating { get; set; }
    }
}
