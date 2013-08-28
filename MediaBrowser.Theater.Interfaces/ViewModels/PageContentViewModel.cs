using System;

namespace MediaBrowser.Theater.Interfaces.ViewModels
{
    public class PageContentViewModel : BaseViewModel, IDisposable
    {
        private bool _showGlobalContent = true;
        public bool ShowGlobalContent
        {
            get { return _showGlobalContent; }

            set
            {
                var changed = _showGlobalContent != value;

                _showGlobalContent = value;

                if (changed)
                {
                    OnPropertyChanged("ShowGlobalContent");
                }
            }
        }

        private bool _showDefaultPageTitle = true;
        public bool ShowDefaultPageTitle
        {
            get { return _showDefaultPageTitle; }

            set
            {
                var changed = _showDefaultPageTitle != value;

                _showDefaultPageTitle = value;

                if (changed)
                {
                    OnPropertyChanged("ShowDefaultPageTitle");
                }
            }
        }

        private string _pageTitle;
        public string PageTitle
        {
            get { return _pageTitle; }

            set
            {
                var changed = !string.Equals(_pageTitle, value);

                _pageTitle = value;

                if (changed)
                {
                    OnPropertyChanged("PageTitle");
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            
        }
    }
}
