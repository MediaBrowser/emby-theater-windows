using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;
using Microsoft.Expression.Media.Effects;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    /// http://victorcher.blogspot.com/2012/02/wpf-transactions.html
    /// </summary>
    [TemplatePart(Name = "PART_ActiveContentPresenter", Type = typeof (ContentPresenter))]
    [TemplatePart(Name = "PART_TransitionContentPresenter", Type = typeof(ContentPresenter))]
    public class ExtendedContentControl
        : ContentControl
    {
        private ContentPresenter _activeContentPresenter;
        private ContentPresenter _transitionContentPresenter;

        private readonly SerialTaskQueue _taskQueue;

        /// <summary>
        ///     Initializes static members of the <see cref="ExtendedContentControl" /> class.
        /// </summary>
        static ExtendedContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ExtendedContentControl), new FrameworkPropertyMetadata(typeof (ExtendedContentControl)));
            ContentProperty.OverrideMetadata(typeof (ExtendedContentControl), new FrameworkPropertyMetadata(OnContentPropertyChanged));
        }

        public ExtendedContentControl()
        {
            _taskQueue = new SerialTaskQueue();
        }

        private ContentPresenter ActiveContentPresenter
        {
            get { return _activeContentPresenter; }
            set
            {
                _activeContentPresenter = value;
                SetContent(Content);
            }
        }

        private ContentPresenter TransitionContentPresenter
        {
            get { return _transitionContentPresenter; }
            set
            {
                _transitionContentPresenter = value;
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
            ActiveContentPresenter = GetTemplateChild("PART_ActiveContentPresenter") as ContentPresenter;
            TransitionContentPresenter = GetTemplateChild("PART_TransitionContentPresenter") as ContentPresenter;
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

        private void SetContent(object content)
        {
//            _taskQueue.Enqueue(async () => {
//                Task task = null;
//                Action action = () => task = ChangeContent(content);
//                await action.OnUiThreadAsync();
//                await task;
//            });
            ChangeContent(content);
        }

        private async Task ChangeContent(object content)
        {
            if (ActiveContentPresenter == null) {
                return;
            }

            var oldContentTransition = TransitionOffCurrentContent();

            var initializable = content as IRequiresInitialization;
            if (initializable != null && !initializable.IsInitialized) {
                await initializable.Initialize();
            }

            var hasActivity = content as IHasActivityStatus;
            if (hasActivity != null) {
                hasActivity.IsActive = true;
            }

            var newContentTransition = TransitionOnNewContent(content);

            await oldContentTransition;
            await newContentTransition;
        }

        private Task TransitionOffCurrentContent()
        {
            var hasActivity = ActiveContentPresenter.Content as IHasActivityStatus;
            if (hasActivity != null) {
                hasActivity.IsActive = false;
            }

            if (TransitionContentPresenter == null ||
                TransitionAnimation == null ||
                TransitionType == null ||
                VisualTreeHelper.GetChildrenCount(ActiveContentPresenter) == 0) 
            {
                ActiveContentPresenter.Content = null;
                return Task.FromResult<object>(null);
            }

            var content = VisualTreeHelper.GetChild(ActiveContentPresenter, 0) as FrameworkElement;
            if (content == null) {
                return Task.FromResult<object>(null);
            }

            var bmp = new RenderTargetBitmap((int) ActiveContentPresenter.ActualWidth,
                                             (int) ActiveContentPresenter.ActualHeight,
                                             96, 96, PixelFormats.Pbgra32);

            bmp.Render(content);

            var taskSource = new TaskCompletionSource<object>();

            var transitionEffect = (TransitionEffect)TransitionType.Clone();

            DoubleAnimation da = TransitionAnimation.Clone();
            da.From = 0;
            da.To = 1;
            da.FillBehavior = FillBehavior.HoldEnd;
            da.Completed += (s, e) =>
            {
                if (TransitionContentPresenter.Effect == transitionEffect) {
                    TransitionContentPresenter.Effect = null;
                }

                taskSource.SetResult(null);
            };

            transitionEffect.OldImage = new ImageBrush(bmp);
            transitionEffect.BeginAnimation(TransitionEffect.ProgressProperty, da);

            TransitionContentPresenter.Effect = transitionEffect;
            TransitionContentPresenter.Content = new Rectangle { Width = bmp.Width, Height = bmp.Height, Fill = new SolidColorBrush(Colors.Transparent) };
            ActiveContentPresenter.Content = null;
            
            return taskSource.Task;
        }

        private Task TransitionOnNewContent(object content)
        {
            ActiveContentPresenter.Content = content;

            if (TransitionAnimation == null || TransitionType == null || content == null) {
                return Task.FromResult<object>(null);
            }

            var taskSource = new TaskCompletionSource<object>();

            var transitionEffect = (TransitionEffect)TransitionType.Clone();

            DoubleAnimation da = TransitionAnimation.Clone();
            da.From = 0;
            da.To = 1;
            da.FillBehavior = FillBehavior.HoldEnd;
            da.Completed += (s, e) =>
            {
                if (ActiveContentPresenter.Effect == transitionEffect) {
                    ActiveContentPresenter.Effect = null;
                }

                taskSource.SetResult(null);
            };

            transitionEffect.OldImage = new VisualBrush(new Grid());
            transitionEffect.BeginAnimation(TransitionEffect.ProgressProperty, da);

            ActiveContentPresenter.Effect = transitionEffect;

            return taskSource.Task;
        }

//        private void ChangeDisplayedContent(object content)
//        {
//            if (_activeContentPresenter.Content == null) {
//                _activeContentPresenter.Content = content;
//                return;
//            }
//
//            FrameworkElement oldContentVisual;
//
//            try {
//                oldContentVisual = VisualTreeHelper.GetChild(_activeContentPresenter, 0) as FrameworkElement;
//            }
//            catch {
//                _activeContentPresenter.Content = content;
//                return;
//            }
//
//            TransitionEffect transitionEffect = TransitionType;
//            int renderingTier = (RenderCapability.Tier >> 16);
//
//            if (renderingTier < 2 || transitionEffect == null) {
//                _activeContentPresenter.Content = content;
//                return;
//            }
//
//            transitionEffect = (TransitionEffect) transitionEffect.Clone();
//
//            DoubleAnimation da = TransitionAnimation.Clone();
//            da.From = 0;
//            da.To = 1;
//            da.FillBehavior = FillBehavior.HoldEnd;
//            da.Completed += (s, e) => {
//                if (_activeContentPresenter.Effect == transitionEffect) {
//                    _activeContentPresenter.Effect = null;
//                }
//            };
//
//            transitionEffect.OldImage = new VisualBrush(oldContentVisual);
//            transitionEffect.BeginAnimation(TransitionEffect.ProgressProperty, da);
//
//            _activeContentPresenter.Effect = transitionEffect;
//            _activeContentPresenter.Content = content;
//        }
    }
}