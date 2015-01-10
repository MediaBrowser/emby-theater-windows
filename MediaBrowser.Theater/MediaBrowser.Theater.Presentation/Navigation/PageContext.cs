using System;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.Presentation.Navigation
{
    public class PageContext<T> 
        : NavigationContext where T : BaseViewModel
    {
        private readonly IApplicationHost _appHost;
        private readonly IPresenter _presenter;

        private T _viewModel;

        public T ViewModel
        {
            get { return _viewModel; }
            private set
            {
                _viewModel = value;

                if (_viewModel != null) {
                    OnViewModelCreated(_viewModel);
                }
            }
        }

        public event Action<T> ViewModelCreated;

        protected virtual void OnViewModelCreated(T obj)
        {
            Action<T> handler = ViewModelCreated;
            if (handler != null) {
                handler(obj);
            }
        }

        public PageContext(IApplicationHost appHost, IPresenter presenter)
            : base(appHost)
        {
            _appHost = appHost;
            _presenter = presenter;
        }

        public override async Task Activate()
        {
            if (ViewModel == null || !ViewModel.IsActive)
            {
                ViewModel = _appHost.TryResolve<T>();
            }

            await _presenter.ShowPage(ViewModel);
        }
    }
}
