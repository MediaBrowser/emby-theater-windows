using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace MediaBrowser.Theater.Core.Login
{
    /// <summary>
    /// Interaction logic for ManualLoginPage.xaml
    /// </summary>
    public partial class ManualLoginPage : BasePage, ILoginPage
    {
        protected ISessionManager SessionManager { get; private set; }
        protected IPresentationManager PresentationManager { get; private set; }
        protected ITheaterConfigurationManager ConfigurationManager { get; private set; }

        public ManualLoginPage(string initialUsername, bool? isAutoLoginChecked, ISessionManager sessionManager, IPresentationManager presentationManager, ITheaterConfigurationManager configManager)
        {
            PresentationManager = presentationManager;
            SessionManager = sessionManager;
            ConfigurationManager = configManager;
            InitializeComponent();

            TxtUsername.Text = initialUsername;
            ChkAutoLogin.IsChecked = isAutoLoginChecked;

            Loaded += LoginPage_Loaded;
            BtnSubmit.Click += BtnSubmit_Click;
        }

        void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            PresentationManager.SetDefaultPageTitle();
        }

        protected override void FocusOnFirstLoad()
        {
            if (!string.IsNullOrEmpty(TxtUsername.Text))
            {
                TxtPassword.Focus();
            }
            else
            {
                base.FocusOnFirstLoad();
            }
        }

        async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await SessionManager.Login(TxtUsername.Text, TxtPassword.Password);

                //If login sucessful and auto login checkbox is ticked then save the auto-login config
                if (ChkAutoLogin.IsChecked == true)
                {
                    ConfigurationManager.Configuration.AutoLoginConfiguration.UserName = TxtUsername.Text;
                    ConfigurationManager.Configuration.AutoLoginConfiguration.UserPasswordHash = Convert.ToBase64String(SessionManager.ComputeHash(""));
                    ConfigurationManager.SaveConfiguration();
                }
            }
            catch (Exception ex)
            {
                PresentationManager.ShowMessage(new MessageBoxInfo
                {
                    Caption = "Login Failure",
                    Text = ex.Message,
                    Icon = MessageBoxIcon.Error
                });
            }
        }
    }
}
