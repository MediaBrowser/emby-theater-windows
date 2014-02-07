using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Theater.Api.Theming.ViewModels;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
{
    public interface IWizardPage : INotifyPropertyChanged
    {
        bool CanMoveNext { get; }
        bool HasCustomNextPage { get; }
        IWizardPage Next();
    }

    public enum NextActionType
    {
        Next,
        Finish
    }

    public enum WizardCompletionStatus
    {
        Finished,
        Canceled
    }

    public class WizardViewModel
        : BaseViewModel
    {
        private class PageRun
        {
            private readonly Stack<IWizardPage> _previousPages;
            private IWizardPage _currentPage;

            public IWizardPage CurrentPage
            {
                get { return _currentPage; }

            }

            public PageRun(IWizardPage page)
            {
                _currentPage = page;
                _previousPages = new Stack<IWizardPage>();
            }

            public bool IsFinished {
                get { return !_currentPage.HasCustomNextPage; }
            }

            public bool HasPrevious {
                get { return _previousPages.Count > 0; }
            }

            public void Next()
            {
                _previousPages.Push(_currentPage);
                _currentPage = _currentPage.Next();
            }

            public void Previous()
            {
                _currentPage = _previousPages.Pop();
            }
        }

        private readonly List<PageRun> _runs;

        private int _currentRun;
        private IWizardPage _currentPage;

        public event Action<WizardCompletionStatus> Completed;

        protected virtual void OnCompleted(WizardCompletionStatus obj)
        {
            Action<WizardCompletionStatus> handler = Completed;
            if (handler != null) {
                handler(obj);
            }
        }

        public IWizardPage CurrentPage
        {
            get { return _currentPage; }
            private set
            {
                if (Equals(value, _currentPage)) {
                    return;
                }

                if (_currentPage != null) {
                    _currentPage.PropertyChanged -= CurrentPagePropertyChanged;
                }

                _currentPage = value;

                if (_currentPage != null) {
                    _currentPage.PropertyChanged += CurrentPagePropertyChanged;
                }

                OnPropertyChanged();
                UpdateButtonStates();
            }
        }

        void CurrentPagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasCustomNextPage") {
                OnPropertyChanged("NextAction");
            }
        }

        private void UpdateButtonStates()
        {
            OnPropertyChanged("IsPreviousAvailable");
            OnPropertyChanged("NextAction");
        }
        
        public bool IsPreviousAvailable
        {
            get { return _currentRun > 0 || _runs[_currentRun].HasPrevious; }
        }

        public NextActionType NextAction
        {
            get { return _currentRun == _runs.Count - 1 && _runs[_currentRun].IsFinished ? NextActionType.Finish : NextActionType.Next; }
        }

        public ICommand NextCommand { get; private set; }
        public ICommand PreviousCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public WizardViewModel(IEnumerable<IWizardPage> pages)
        {
            _runs = pages.Select(p => new PageRun(p)).ToList();
            CurrentPage = _runs.First().CurrentPage;

            NextCommand = new RelayCommand(arg => MoveNext());
            PreviousCommand = new RelayCommand(arg => MoveBack());
            CancelCommand = new RelayCommand(arg => Cancel());
        }

        private void MoveNext()
        {
            var run = _runs[_currentRun];

            if (run.IsFinished) {
                if (_currentRun == _runs.Count - 1) {
                    OnCompleted(WizardCompletionStatus.Finished);
                    return;
                }

                _currentRun++;
                CurrentPage = _runs[_currentRun].CurrentPage;
            } else {
                run.Next();
                CurrentPage = run.CurrentPage;
            }
        }

        private void MoveBack()
        {
            var run = _runs[_currentRun];

            if (run.HasPrevious) {
                run.Previous();
                CurrentPage = run.CurrentPage;
            } else {
                _currentRun--;
                CurrentPage = _runs[_currentRun].CurrentPage;
            }
        }

        private void Cancel()
        {
            OnCompleted(WizardCompletionStatus.Canceled);
        }
    }
}
