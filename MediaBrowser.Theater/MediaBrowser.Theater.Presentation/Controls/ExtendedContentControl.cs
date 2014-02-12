using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Theater.Api.UserInterface.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.Presentation.Controls
{
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof (ContentPresenter))]
    public class ExtendedContentControl
        : ContentControl
    {
        /// <summary>
        ///     The _content presenter
        /// </summary>
        private ContentPresenter _contentPresenter;

        private ContentPresenter ContentPresenter
        {
            get { return _contentPresenter; }
            set
            {
                _contentPresenter = value;
                SetContent(Content);
            }
        }

        /// <summary>
        ///     Initializes static members of the <see cref="ExtendedContentControl" /> class.
        /// </summary>
        static ExtendedContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ExtendedContentControl), new FrameworkPropertyMetadata(typeof (ExtendedContentControl)));
            ContentProperty.OverrideMetadata(typeof (ExtendedContentControl), new FrameworkPropertyMetadata(OnContentPropertyChanged));
        }

        /// <summary>
        ///     When overridden in a derived class, is invoked whenever application code or internal processes call
        ///     <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ContentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
        }

        /// <summary>
        ///     Called when [content property changed].
        /// </summary>
        /// <param name="dp">The dp.</param>
        /// <param name="args">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnContentPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            var control = (ExtendedContentControl) dp;
            object content = args.NewValue;

            control.SetContent(content);
        }

        private async void SetContent(object content)
        {
            if (ContentPresenter == null) {
                return;
            }

            var initializable = content as IRequiresInitialization;
            if (initializable != null && !initializable.IsInitialized) {
                await initializable.Initialize();
            }

            if (Content == content) {
                var currentActivatable = _contentPresenter.Content as IHasActivityStatus;
                if (currentActivatable != null) {
                    currentActivatable.IsActive = false;
                }

                var activatable = content as IHasActivityStatus;
                if (activatable != null) {
                    activatable.IsActive = true;
                }

                ContentPresenter.Content = content;
            }
        }
    }
}