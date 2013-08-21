using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class ItemReviewViewModel : BaseViewModel
    {
        private ItemReview _review;
        public ItemReview Review
        {
            get { return _review; }

            set
            {
                var changed = _review != value;

                _review = value;

                if (changed)
                {
                    OnPropertyChanged("Review");
                    OnPropertyChanged("Caption");
                    OnPropertyChanged("Publisher");
                    OnPropertyChanged("ReviewerName");
                    OnPropertyChanged("Url");
                    OnPropertyChanged("Score");
                    OnPropertyChanged("Likes");
                    OnPropertyChanged("Date");
                    OnPropertyChanged("CombinedSource");
                }
            }
        }

        public string Caption
        {
            get { return _review == null ? null : _review.Caption; }
        }

        public string Publisher
        {
            get { return _review == null ? null : _review.Publisher; }
        }

        public string ReviewerName
        {
            get { return _review == null ? null : _review.ReviewerName; }
        }

        public string Url
        {
            get { return _review == null ? null : _review.Url; }
        }

        public float? Score
        {
            get { return _review == null ? null : _review.Score; }
        }

        public bool? Likes
        {
            get { return _review == null ? null : _review.Likes; }
        }

        public DateTime? Date
        {
            get { return _review == null ? (DateTime?)null : _review.Date; }
        }

        public string CombinedSource
        {
            get
            {
                var review = _review;

                if (review == null)
                {
                    return null;
                }

                var source = string.Format("{0}. {1}", review.Publisher ?? string.Empty, review.Date.ToShortDateString());

                if (!string.IsNullOrEmpty(review.ReviewerName))
                {
                    source = review.ReviewerName + ", " + source;
                }

                return source;
            }
        }
    }
}
