using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public class ManualLoginViewModel
        : BaseValidatingViewModel
    {
        private string _password;
        private bool _rememberMe;
        private string _username;
        private bool _isInvalidLoginDetails;

        public ManualLoginViewModel(ISessionManager sessionManager, ILogManager logManager)
        {
            var logger = logManager.GetLogger("Login");

            LoginCommand = new RelayCommand(async arg => {
                await Validate();

                try {
                    IsInvalidLoginDetails = false;
                    await sessionManager.Login(Username, Password, RememberMe);
                }
                catch (UnauthorizedAccessException) {
                    IsInvalidLoginDetails = true;
                }
                catch (Exception e) {
                    logger.ErrorException("Error while attempting to login", e);
                }
            });
        }

        public ICommand LoginCommand { get; private set; }

        public bool IsInvalidLoginDetails
        {
            get { return _isInvalidLoginDetails; }
            private set
            {
                if (Equals(_isInvalidLoginDetails, value)) {
                    return;
                }

                _isInvalidLoginDetails = value;
                OnPropertyChanged();
            }
        }

        [Required]
        public string Username
        {
            get { return _username; }
            set
            {
                if (Equals(_username, value)) {
                    return;
                }

                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (Equals(_password, value)) {
                    return;
                }

                _password = value;
                OnPropertyChanged();
            }
        }

        public bool RememberMe
        {
            get { return _rememberMe; }
            set
            {
                if (Equals(_rememberMe, value)) {
                    return;
                }

                _rememberMe = value;
                OnPropertyChanged();
            }
        }
    }
}