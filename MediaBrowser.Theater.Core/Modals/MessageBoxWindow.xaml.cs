using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace MediaBrowser.Theater.Core.Modals
{
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : BaseModalWindow
    {
        public MessageBoxWindow(MessageBoxInfo options)
            : base()
        {
            InitializeComponent();

            Loaded += MessageBoxWindow_Loaded;

            DataContext = new MessageBoxViewModel(options, result =>
            {
                MessageBoxResult = result;
                CloseModal();
            });
        }

        void MessageBoxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        public MessageBoxResult MessageBoxResult { get; private set; }
    }
}
