using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ItemInfoViewModel
        : BaseViewModel
    {
        private readonly BaseItemDto _item;
        private Func<BaseItemDto, string> _displayNameGenerator;

        public ItemInfoViewModel(BaseItemDto item)
        {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            _item = item;
            DisplayNameGenerator = ItemTileViewModel.GetDisplayNameWithAiredSpecial;
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

        public string ItemType
        {
            get { return _item.Type; }
        }

        public bool ShowStats
        {
            get { return _item.Type != "Person"; }
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

        public bool HasDate
        {
            get { return !string.IsNullOrEmpty(Date); }
        }

        public string AudioChannelLayout
        {
            get
            {
                if (_item.MediaStreams != null) {
                    MediaStream stream = _item.MediaStreams
                                              .OrderBy(i => i.Index)
                                              .FirstOrDefault(i => i.Type == MediaStreamType.Audio);

                    if (stream != null) {
                        return stream.ChannelLayout;
                    }
                }

                return null;
            }
        }

        public string AudioCodec
        {
            get
            {

                if (_item != null && _item.MediaStreams != null) {
                    var stream = _item.MediaStreams
                                      .OrderBy(i => i.Index)
                                      .FirstOrDefault(i => i.Type == MediaStreamType.Audio);

                    if (stream != null) {
                        return string.Equals(stream.Codec, "dca", StringComparison.OrdinalIgnoreCase) ? stream.Profile : stream.Codec;
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

        public string Resolution
        {
            get
            {
                if (_item.MediaStreams != null) {
                    MediaStream stream = _item.MediaStreams
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

        public bool HasResolution
        {
            get { return !string.IsNullOrEmpty(Resolution); }
        }

        public string MediaType
        {
            get { return _item != null ? _item.MediaType : null; }
        }

        public string Genres
        {
            get { return _item.Genres != null ? string.Join(" / ", _item.Genres) : string.Empty; }
        }

        public bool HasGenres
        {
            get { return _item.Genres != null && _item.Genres.Count > 0; }
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

                if (_item.AirDays != null && _item.AirDays.Count > 0) {
                    formatStringKey += "Days";
                    formatParameters.Add(_item.AirDays.Select(i => i.Localize()).ToLocalizedList());
                }

                if (!string.IsNullOrEmpty(_item.AirTime)) {
                    formatStringKey += "Time";
                    formatParameters.Add(_item.AirTime); //todo convert series air time to localized time
                }

                if (_item.Studios != null) {
                    StudioDto studio = _item.Studios.FirstOrDefault();
                    if (studio != null) {
                        formatStringKey += "Network";
                        formatParameters.Add(studio.Name);
                    }
                }

                return formatStringKey.LocalizeFormat(formatParameters.ToArray());
            }
        }

        public string SeriesAirTimeLabel
        {
            get
            {
                if ((_item.Status ?? SeriesStatus.Continuing) == SeriesStatus.Continuing) {
                    return "MediaBrowser.Theater.DefaultTheme:Strings:AiringLabel".Localize();
                }

                return "MediaBrowser.Theater.DefaultTheme:Strings:AiredLabel".Localize();
            }
        }

        public bool HasAirTime
        {
            get { return _item.IsType("series"); }
        }

        public string Runtime
        {
            get
            {
                long? ticks = _item.RunTimeTicks;

                if (!ticks.HasValue) {
                    return null;
                }

                double minutes = Math.Round(TimeSpan.FromTicks(ticks.Value).TotalMinutes);
                return "MediaBrowser.Theater.DefaultTheme:Strings:RuntimeMinutes".LocalizeFormat(minutes);
            }
        }

        public bool IsLiked
        {
            get { return _item.UserData != null && (_item.UserData.Likes ?? false); }
        }

        public bool IsFavorite
        {
            get { return _item.UserData != null && _item.UserData.IsFavorite; }
        }

        public bool IsDisliked
        {
            get { return _item.UserData != null && _item.UserData.Likes.HasValue && !_item.UserData.Likes.Value; }
        }

        public string OfficialRating
        {
            get { return _item.OfficialRating; }
        }

        public float CommunityRating
        {
            get { return _item.CommunityRating ?? 0; }
        }

        public bool HasCommunityRating
        {
            get { return _item.CommunityRating.HasValue; }
        }

        public float CriticRating
        {
            get { return _item.CriticRating ?? 0; }
        }

        public bool HasCriticRating
        {
            get { return _item.CriticRating.HasValue; }
        }

        public bool HasPositiveCriticRating
        {
            get { return HasCriticRating && _item.CriticRating >= 60; }
        }

        public bool HasNegativeCriticRating
        {
            get { return HasCriticRating && _item.CriticRating < 60; }
        }

        private bool IsCloseTo(int x, int y)
        {
            return Math.Abs(x - y) <= 20;
        }

        public string Players
        {
            get { return (_item.Players ?? 0).ToString(CultureInfo.InvariantCulture); }
        }

        public bool HasPlayers
        {
            get { return _item.Players.HasValue; }
        }

        public bool IsFolder
        {
            get { return _item.IsFolder; }
        }

        public double PlayedPercentage
        {
            get
            {
                var item = _item;

                if (item != null) {
                    if (item.IsFolder) {
                        return item.PlayedPercentage ?? 0;
                    }

                    if (item.RunTimeTicks.HasValue) {
                        if (item.UserData != null && item.UserData.PlaybackPositionTicks > 0) {
                            double percent = item.UserData.PlaybackPositionTicks;
                            percent /= item.RunTimeTicks.Value;

                            return percent*100;
                        }
                    }
                }

                return 0;
            }
        }

        public bool IsInProgress
        {
            get
            {
                var progress = PlayedPercentage;
                return progress > 0 && progress < 100;
            }
        }
    }
}