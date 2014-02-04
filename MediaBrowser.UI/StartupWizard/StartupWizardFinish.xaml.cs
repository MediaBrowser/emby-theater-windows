using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Windows;

namespace MediaBrowser.UI.StartupWizard
{
    /// <summary>
    /// Interaction logic for StartupWizardFinish.xaml
    /// </summary>
    public partial class StartupWizardFinish : BasePage
    {
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        
        public StartupWizardFinish(INavigationService nav, IPresentationManager presentation)
        {
            _nav = nav;
            _presentation = presentation;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += StartupWizardPage_Loaded;
            BtnFinish.Click += BtnFinish_Click;
            BtnBack.Click += BtnBack_Click;
        }

        public override void FocusOnFirstLoad()
        {
            BtnFinish.Focus();
        }

        async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            await _nav.NavigateBack();
        }

        async void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            await _nav.NavigateToLoginPage();
            _nav.ClearHistory();
        }

        void StartupWizardPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetDefaultPageTitle();
        }

    }
}
