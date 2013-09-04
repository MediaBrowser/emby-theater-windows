using MediaBrowser.Theater.Presentation.Controls;
using System;
using System.Windows;

namespace MediaBrowser.Theater.Core.Loading
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : BaseWindow
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            AllowsTransparency = true;
            DataContext = this;
        }

        public void Show(Window owner)
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.Manual;

            Width = owner.Width;
            Height = owner.Height;
            Top = owner.Top;
            Left = owner.Left;
            WindowState = owner.WindowState;
            Owner = owner;

            Show();
        }
    }
}
