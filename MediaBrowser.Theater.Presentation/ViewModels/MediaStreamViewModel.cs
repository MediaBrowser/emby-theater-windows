using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class MediaStreamViewModel : BaseViewModel
    {
        public BaseItemDto OwnerItem { get; set; }

        private MediaStream _item;
        public MediaStream MediaStream
        {
            get { return _item; }

            set
            {
                var changed = _item != value;

                _item = value;

                if (changed)
                {
                    OnPropertyChanged("MediaStream");
                    OnPropertyChanged("Language");
                    OnPropertyChanged("AspectRatio");
                    OnPropertyChanged("BitRate");
                    OnPropertyChanged("Channels");
                    OnPropertyChanged("Codec");
                    OnPropertyChanged("Height");
                    OnPropertyChanged("IsDefault");
                    OnPropertyChanged("IsExternal");
                    OnPropertyChanged("IsForced");
                    OnPropertyChanged("Profile");
                    OnPropertyChanged("Type");
                    OnPropertyChanged("Width");
                    OnPropertyChanged("SampleRate");
                    OnPropertyChanged("Attributes");
                }
            }
        }

        public MediaStreamType Type
        {
            get { return _item == null ? MediaStreamType.Video : _item.Type; }
        }

        public int? Width
        {
            get { return _item == null ? null : _item.Width; }
        }

        public string Profile
        {
            get { return _item == null ? null : string.Equals(_item.Codec, "dca", System.StringComparison.OrdinalIgnoreCase) ? null : _item.Profile; }
        }

        public string Language
        {
            get { return _item == null ? null : _item.Language; }
        }

        public string AspectRatio
        {
            get { return _item == null ? null : _item.AspectRatio; }
        }

        public int? BitRate
        {
            get { return _item == null ? null : _item.BitRate; }
        }

        public int? SampleRate
        {
            get { return _item == null ? null : _item.SampleRate; }
        }
        
        public int? Channels
        {
            get { return _item == null ? null : _item.Channels; }
        }

        public string Codec
        {
            get { return _item == null ? null : string.Equals(_item.Codec, "dca", System.StringComparison.OrdinalIgnoreCase) ? _item.Profile : _item.Codec; }
        }

        public int? Height
        {
            get { return _item == null ? null : _item.Height; }
        }

        public bool IsDefault
        {
            get { return _item != null && _item.IsDefault; }
        }

        public bool IsExternal
        {
            get { return _item != null && _item.IsExternal; }
        }

        public bool IsForced
        {
            get { return _item != null && _item.IsForced; }
        }

        public string Resolution
        {
            get
            {
                var stream = _item;

                if (stream != null)
                {
                    if (stream.Width.HasValue && stream.Height.HasValue)
                    {
                        if (IsCloseTo(stream.Width.Value, 1920))
                        {
                            return "1080p";
                        }
                        if (IsCloseTo(stream.Width.Value, 1280))
                        {
                            return "720p";
                        }
                        if (IsCloseTo(stream.Width.Value, 720))
                        {
                            return "480p";
                        }
                        return stream.Width + "x" + stream.Height;
                    }
                }

                return null;
            }
        }

        private bool IsCloseTo(int x, int y)
        {
            return Math.Abs(x - y) <= 10;
        }

        public List<string> Attributes
        {
            get
            {
                var list = new List<string>();

                if (_item == null)
                {
                    return list;
                }

                if (Type != MediaStreamType.Video && !string.IsNullOrEmpty(Language))
                {
                    list.Add(Language);
                }
                if (!string.IsNullOrEmpty(Resolution))
                {
                    list.Add(Resolution);
                }
                if (!string.IsNullOrEmpty(Codec))
                {
                    list.Add(Codec.ToUpper());
                }
                if (!string.IsNullOrEmpty(AspectRatio))
                {
                    list.Add(AspectRatio);
                }
                if (OwnerItem.IsAudio)
                {
                    if (BitRate.HasValue)
                    {
                        list.Add(BitRate.Value.ToString(CultureInfo.CurrentUICulture) + " kbps");
                    }
                }
                if (Channels.HasValue)
                {
                    list.Add(Channels.Value + " Channel");
                }
                if (SampleRate.HasValue)
                {
                    list.Add(SampleRate.Value.ToString(CultureInfo.CurrentUICulture) + " khz");
                }

                if (IsDefault)
                {
                    list.Add("Default");
                }
                if (IsForced)
                {
                    list.Add("Forced");
                }
                if (IsExternal)
                {
                    list.Add("External");
                }

                return list;
            }
        }
    }
}
