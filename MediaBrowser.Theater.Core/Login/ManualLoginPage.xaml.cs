using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Pages;
using System;
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

        public ManualLoginPage(string initialUsername, ISessionManager sessionManager, IPresentationManager presentationManager)
        {
            PresentationManager = presentationManager;
            SessionManager = sessionManager;
            InitializeComponent();

            TxtUsername.Text = initialUsername;

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
