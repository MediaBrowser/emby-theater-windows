using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;
using Microsoft.Expression.Media.Effects;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    /// http://victorcher.blogspot.com/2012/02/wpf-transactions.html
    /// </summary>
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof (ContentPresenter))]
    public class ExtendedContentControl
        : ContentControl
    {
        /// <summary>
        ///     The _content presenter
        /// </summary>
        private ContentPresenter _contentPresenter;

        /// <summary>
        ///     Initializes static members of the <see cref="ExtendedContentControl" /> class.
        /// </summary>
        static ExtendedContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ExtendedContentControl), new FrameworkPropertyMetadata(typeof (ExtendedContentControl)));
            ContentProperty.OverrideMetadata(typeof (ExtendedContentControl), new FrameworkPropertyMetadata(OnContentPropertyChanged));
        }

        private ContentPresenter ContentPresenter
        {
            get { return _contentPresenter; }
            set
            {
                _contentPresenter = value;
                SetContent(Content);
            }
        }

        #region DP TransitionType

        // Using a DependencyProperty as the backing store for TransitionType.  This enables animation, styling, binding, etc...
        /// <summary>
        ///     The transition type property
        /// </summary>
        public static readonly DependencyProperty TransitionTypeProperty =
            DependencyProperty.Register("TransitionType", typeof (TransitionEffect), typeof (ExtendedContentControl),
                                        new UIPropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the type of the transition.
        /// </summary>
        /// <value>The type of the transition.</value>
        public TransitionEffect TransitionType
        {
            get { return (TransitionEffect) GetValue(TransitionTypeProperty); }
            set { SetValue(TransitionTypeProperty, value); }
        }

        #endregion DP TransitionType

        #region DP Transition Animation

        // Using a DependencyProperty as the backing store for TransitionAnimation.  This enables animation, styling, binding, etc...
        /// <summary>
        ///     The transition animation property
        /// </summary>
        public static readonly DependencyProperty TransitionAnimationProperty =
            DependencyProperty.Register("TransitionAnimation", typeof (DoubleAnimation), typeof (ExtendedContentControl), new UIPropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the transition animation.
        /// </summary>
        /// <value>The transition animation.</value>
        public DoubleAnimation TransitionAnimation
        {
            get { return (DoubleAnimation) GetValue(TransitionAnimationProperty); }
            set { SetValue(TransitionAnimationProperty, value); }
        }

        #endregion DP Transition Animation

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

                ChangeDisplayedContent(content);
            }
        }

        private void ChangeDisplayedContent(object content)
        {
            if (_contentPresenter.Content == null) {
                _contentPresenter.Content = content;
                return;
            }

            FrameworkElement oldContentVisual;

            try {
                oldContentVisual = VisualTreeHelper.GetChild(_contentPresenter, 0) as FrameworkElement;
            }
            catch {
                _contentPresenter.Content = content;
                return;
            }

            TransitionEffect transitionEffect = TransitionType;
            int renderingTier = (RenderCapability.Tier >> 16);

            if (renderingTier < 2 || transitionEffect == null) {
                _contentPresenter.Content = content;
                return;
            }

            transitionEffect = (TransitionEffect) transitionEffect.Clone();

            DoubleAnimation da = TransitionAnimation.Clone();
            da.From = 0;
            da.To = 1;
            da.FillBehavior = FillBehavior.HoldEnd;

            transitionEffect.OldImage = new VisualBrush(oldContentVisual);
            transitionEffect.BeginAnimation(TransitionEffect.ProgressProperty, da);

            _contentPresenter.Effect = transitionEffect;
            _contentPresenter.Content = content;
        }
    }
}