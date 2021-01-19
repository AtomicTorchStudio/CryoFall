namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class Scalebox : BaseContentControl
    {
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            nameof(Scale),
            typeof(float),
            typeof(Scalebox),
            new PropertyMetadata(1f, ScaleMultChanged));

        private ContentPresenter contentPresenter;

        private ScaleTransform contentPresenterScaleTransform;

        private FrameworkElement currentChild;

        static Scalebox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Scalebox),
                new FrameworkPropertyMetadata(typeof(Scalebox)));
        }

        public float Scale
        {
            get => (float)this.GetValue(ScaleProperty);
            set => this.SetValue(ScaleProperty, value);
        }

        protected override void OnLoaded()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.contentPresenter = templateRoot.GetByName<ContentPresenter>("ContentPresenter");
            this.contentPresenterScaleTransform = new ScaleTransform(1f, 1f);
            this.contentPresenter.RenderTransform = this.contentPresenterScaleTransform;

            // force stretch mode so we will know actual size of this viewbox
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.SizeChanged += this.SizeChangedHandler;

            this.RefreshViewBoxMargins();
        }

        protected override void OnUnloaded()
        {
            this.SizeChanged -= this.SizeChangedHandler;
        }

        private static void ScaleMultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scalebox)d).RefreshViewBoxMargins();
        }

        private void OnCurrentChildOnSizeChanged(object o, SizeChangedEventArgs e)
        {
            if (e.WidthChanged && this.currentChild.HorizontalAlignment != HorizontalAlignment.Stretch
                || e.HeightChanged && this.currentChild.VerticalAlignment != VerticalAlignment.Stretch)
            {
                this.RefreshViewBoxMargins();
            }
        }

        private void RefreshViewBoxMargins()
        {
            if (this.contentPresenter is null)
            {
                return;
            }

            var child = (FrameworkElement)this.contentPresenter.Content;
            if (this.currentChild != child)
            {
                // child changed
                if (this.currentChild is not null)
                {
                    this.currentChild.SizeChanged -= this.OnCurrentChildOnSizeChanged;
                }

                this.currentChild = child;

                if (this.currentChild is not null)
                {
                    this.currentChild.SizeChanged += this.OnCurrentChildOnSizeChanged;
                }
            }

            if (this.currentChild is null)
            {
                return;
            }

            var margin = child.Margin;
            var targetWidth = child.ActualWidth + margin.Left + margin.Right;
            var targetHeight = child.ActualHeight + margin.Top + margin.Bottom;

            var scale = this.Scale;

            var frameActualWidth = this.ActualWidth;
            var frameActualHeight = this.ActualHeight;

            var childActualWidth = targetWidth * scale;
            var childActualHeight = targetHeight * scale;

            double left, top;
            switch (child.HorizontalAlignment)
            {
                case HorizontalAlignment.Stretch:
                    left = 0;
                    childActualWidth = frameActualWidth / scale;
                    child.Width = childActualWidth;
                    break;

                case HorizontalAlignment.Left:
                    left = 0;
                    break;

                case HorizontalAlignment.Right:
                    left = frameActualWidth - childActualWidth;
                    break;

                case HorizontalAlignment.Center:
                default:
                    left = (frameActualWidth - childActualWidth) / 2f;
                    break;
            }

            switch (child.VerticalAlignment)
            {
                case VerticalAlignment.Stretch:
                    top = 0;
                    childActualHeight = frameActualHeight / scale;
                    child.Height = childActualHeight;
                    break;

                case VerticalAlignment.Top:
                    top = 0;
                    break;

                case VerticalAlignment.Bottom:
                    top = frameActualHeight - childActualHeight;
                    break;

                case VerticalAlignment.Center:
                default:
                    top = (frameActualHeight - childActualHeight) / 2f;
                    break;
            }

            // set offset
            Canvas.SetLeft(this.contentPresenter, (float)left);
            Canvas.SetTop(this.contentPresenter, (float)top);

            // set scale
            this.contentPresenterScaleTransform.ScaleX = scale;
            this.contentPresenterScaleTransform.ScaleY = scale;
        }

        private void SizeChangedHandler(object o, SizeChangedEventArgs e)
        {
            if (this.currentChild is null)
            {
                return;
            }

            this.RefreshViewBoxMargins();
        }
    }
}