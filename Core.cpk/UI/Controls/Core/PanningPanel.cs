namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class PanningPanel : BaseItemsControl
    {
        public const double MouseScrollWheelZoomSpeed = 0.0015;

        private const double ZoomAnimationDurationSeconds = 0.1;

        public static readonly DependencyProperty DefaultZoomProperty = DependencyProperty.Register(
            nameof(DefaultZoom),
            typeof(double),
            typeof(PanningPanel),
            new PropertyMetadata(1d, DefaultZoomPropertyChanged));

        public static readonly DependencyProperty MaxZoomProperty = DependencyProperty.Register(
            nameof(MaxZoom),
            typeof(double),
            typeof(PanningPanel),
            new PropertyMetadata(1d, MinOrMaxZoomPropertyChanged));

        public static readonly DependencyProperty VisibilityZoomSliderProperty =
            DependencyProperty.Register(nameof(VisibilityZoomSlider),
                                        typeof(Visibility),
                                        typeof(PanningPanel),
                                        new PropertyMetadata(default(Visibility)));

        public static readonly DependencyProperty MinZoomProperty = DependencyProperty.Register(
            nameof(MinZoom),
            typeof(double),
            typeof(PanningPanel),
            new PropertyMetadata(0.5, MinOrMaxZoomPropertyChanged));

        public static readonly DependencyProperty PanningHeightProperty = DependencyProperty.Register(
            nameof(PanningHeight),
            typeof(double),
            typeof(PanningPanel),
            new PropertyMetadata(default(double), PanningSizeChangedHandler));

        public static readonly DependencyProperty PanningWidthProperty = DependencyProperty.Register(
            nameof(PanningWidth),
            typeof(double),
            typeof(PanningPanel),
            new PropertyMetadata(default(double), PanningSizeChangedHandler));

        public static readonly DependencyProperty IsAutoCalculatingMinZoomProperty =
            DependencyProperty.Register(nameof(IsAutoCalculatingMinZoom),
                                        typeof(bool),
                                        typeof(PanningPanel),
                                        new PropertyMetadata(defaultValue: true));

        private ClickHandlerHelper clickHandlerHelper;

        private Vector2D clipperSize;

        private RectangleGeometry clippingRect;

        private Canvas content;

        private UIElement contentItems;

        private Storyboard contentScaleStoryboard;

        private ScaleTransform contentScaleTransform;

        private Storyboard contentTranslateStoryboard;

        private TranslateTransform contentTranslateTransform;

        private Canvas contentWrapper;

        private bool isListeningToScaleEvents = true;

        private bool isMouseButtonDown;

        private Panel mouseEventsRoot;

        private Vector2D? mouseHoldAbsolutePosition;

        private BoundsDouble offsetBounds;

        private BoundsDouble? panningBounds;

        private double panningHeight;

        private double panningWidth;

        private Vector2D? requireCenterOnPoint;

        private Slider sliderScale;

        static PanningPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PanningPanel),
                new FrameworkPropertyMetadata(typeof(PanningPanel)));
        }

        public event Action MouseHold;

        public event Action<PanningPanel, MouseEventArgs> MouseLeftButtonClick;

        public event Action<(double newZoom, bool isByPlayersInput)> ZoomChanged;

        public Func<Vector2D?> CallbackGetSliderZoomCanvasPosition { get; set; }

        public double CurrentAnimatedZoom => this.contentScaleTransform.ScaleX;

        public double CurrentTargetZoom { get; private set; } = 1;

        public double DefaultZoom
        {
            get => (double)this.GetValue(DefaultZoomProperty);
            set => this.SetValue(DefaultZoomProperty, value);
        }

        public bool IsAutoCalculatingMinZoom
        {
            get => (bool)this.GetValue(IsAutoCalculatingMinZoomProperty);
            set => this.SetValue(IsAutoCalculatingMinZoomProperty, value);
        }

        public double MaxZoom
        {
            get => (double)this.GetValue(MaxZoomProperty);
            set => this.SetValue(MaxZoomProperty, value);
        }

        public double MinZoom
        {
            get => (double)this.GetValue(MinZoomProperty);
            set => this.SetValue(MinZoomProperty, value);
        }

        public BoundsDouble PanningBounds
        {
            get => this.panningBounds ?? default;
            set
            {
                if (this.panningBounds.Equals(value))
                {
                    return;
                }

                this.panningBounds = value;

                this.AutoCalculateMinZoom();

                this.UpdateBounds();
            }
        }

        public double PanningHeight
        {
            get => (double)this.GetValue(PanningHeightProperty);
            set => this.SetValue(PanningHeightProperty, value);
        }

        public double PanningWidth
        {
            get => (double)this.GetValue(PanningWidthProperty);
            set => this.SetValue(PanningWidthProperty, value);
        }

        public Visibility VisibilityZoomSlider
        {
            get => (Visibility)this.GetValue(VisibilityZoomSliderProperty);
            set => this.SetValue(VisibilityZoomSliderProperty, value);
        }

        private bool IsClipperSizeCorrect
        {
            get
            {
                double width = this.clipperSize.X,
                       height = this.clipperSize.Y;

                return width > 0
                       && height > 0
                       && !double.IsNaN(width)
                       && !double.IsNaN(height)
                       && !double.IsInfinity(width)
                       && !double.IsInfinity(height);
            }
        }

        public void CenterCanvasContent()
        {
            var centerPoint = (this.panningWidth / 2d,
                               this.panningHeight / 2d);
            this.CenterOnPoint(centerPoint);
        }

        public void CenterOnPoint(Vector2D point)
        {
            if (!this.isLoaded
                || !this.IsClipperSizeCorrect)
            {
                this.requireCenterOnPoint = point;
                return;
            }

            var scale = this.contentScaleTransform.ScaleX;
            point = ((this.clipperSize.X - point.X * 2d * scale) / 2d,
                     (this.clipperSize.Y - point.Y * 2d * scale) / 2d);

            point = this.ClampOffset(point);
            this.SetOffset(point, isInstant: true);

            this.requireCenterOnPoint = null;
            //Api.Logger.WriteDev($"Centered on point: {point} offset: {newX:F2};{newY:F2}");
        }

        public void Refresh()
        {
            if (!this.isLoaded)
            {
                return;
            }

            this.contentWrapper.UpdateLayout();
            var renderSize = this.contentWrapper.RenderSize;
            this.clipperSize = (renderSize.Width, renderSize.Height);

            if (!this.IsClipperSizeCorrect)
            {
                return;
            }

            this.clippingRect.Rect = new Rect(0, 0, this.clipperSize.X, this.clipperSize.Y);
            //Api.Logger.WriteDev($"Clipping rect: {this.clipperSize.X}x{this.clipperSize.Y}");

            this.AutoCalculateMinZoom();
            this.UpdateBounds();
        }

        public void SetZoom(double newScale)
        {
            this.contentScaleStoryboard?.Remove(this.content);
            this.contentScaleStoryboard?.Stop(this.content);
            this.contentScaleStoryboard = null;

            newScale = MathHelper.Clamp(newScale, this.MinZoom, this.MaxZoom);
            this.CurrentTargetZoom = newScale;

            // clear animation from these properties
            this.contentScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            this.contentScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            // and apply changed values
            this.contentScaleTransform.ScaleX = newScale;
            this.contentScaleTransform.ScaleY = newScale;

            if (this.isLoaded)
            {
                this.UpdateBounds();

                if (this.sliderScale != null)
                {
                    this.isListeningToScaleEvents = false;
                    this.sliderScale.Value = newScale;
                    this.isListeningToScaleEvents = true;
                }
            }

            this.OnZoomChanged(isByPlayersInput: false);
        }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            var layoutRoot = templateRoot.GetByName<Grid>("LayoutRoot");

            this.contentWrapper = layoutRoot.GetByName<Canvas>("CanvasContentWrapper");
            this.content = layoutRoot.GetByName<Canvas>("CanvasContent");
            this.contentItems = layoutRoot.GetByName<Canvas>("CanvasContentItems");

            this.mouseEventsRoot = IsDesignTime ? layoutRoot : Api.Client.UI.LayoutRoot;

            this.contentScaleTransform = new ScaleTransform();
            this.contentTranslateTransform = new TranslateTransform();

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(this.contentScaleTransform);
            transformGroup.Children.Add(this.contentTranslateTransform);
            this.content.RenderTransform = transformGroup;

            this.clippingRect = new RectangleGeometry();
            this.contentWrapper.Clip = this.clippingRect;
            this.sliderScale = layoutRoot.FindName<Slider>("SliderScale");

            this.CurrentTargetZoom = this.DefaultZoom;
        }

        protected override void OnLoaded()
        {
            this.UpdatePanningSize();

            this.SetZoom(this.CurrentTargetZoom);

            if (!IsDesignTime)
            {
                // subscribe to drag/zoom mouse events
                this.contentWrapper.PreviewMouseLeftButtonDown += this.ContentWrapperOnMouseLeftButtonDownHandler;
                this.contentWrapper.MouseWheel += this.ContentWrapperMouseWheelHandler;

                if (this.sliderScale != null)
                {
                    this.sliderScale.ValueChanged += this.SliderScaleValueChangedHandler;
                }

                this.SizeChanged += this.CanvasSizeChangedHandler;

                this.clickHandlerHelper = new ClickHandlerHelper(
                    this,
                    mouseEventArgs => this.MouseLeftButtonClick?.Invoke(this, mouseEventArgs));
            }

            this.Refresh();

            if (this.requireCenterOnPoint.HasValue)
            {
                this.CenterOnPoint(this.requireCenterOnPoint.Value);
            }
            else
            {
                this.CenterCanvasContent();
            }
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.ReleaseMouse();

            // unsubscribe drag/zoom mouse events
            this.contentWrapper.PreviewMouseLeftButtonDown -= this.ContentWrapperOnMouseLeftButtonDownHandler;
            this.contentWrapper.MouseWheel -= this.ContentWrapperMouseWheelHandler;

            if (this.sliderScale != null)
            {
                this.sliderScale.ValueChanged -= this.SliderScaleValueChangedHandler;
            }

            this.SizeChanged -= this.CanvasSizeChangedHandler;

            this.clickHandlerHelper.Dispose();
            this.clickHandlerHelper = null;
        }

        /// <summary>
        /// Zoom to screen point.
        /// </summary>
        protected void ZoomToPoint(double newScale, Vector2D contentWrapperPoint, bool isInstant)
        {
            if (!this.isLoaded
                || this.CurrentTargetZoom == newScale)
            {
                return;
            }

            this.CurrentTargetZoom = newScale;

            var oldScale = this.contentScaleTransform.ScaleX;

            Vector2D currentOffset = (this.contentTranslateTransform.X,
                                      this.contentTranslateTransform.Y);

            // fix on cursor to avoid moving while scaling
            var delta = ((newScale - oldScale) * (currentOffset.X - contentWrapperPoint.X) / oldScale,
                         (newScale - oldScale) * (currentOffset.Y - contentWrapperPoint.Y) / oldScale);

            this.UpdateBounds();
            var offset = currentOffset + delta;
            offset = this.ClampOffset(offset);

            this.mouseHoldAbsolutePosition = null;

            this.contentScaleStoryboard?.Remove(this.content);
            this.contentScaleStoryboard = null;

            if (isInstant)
            {
                this.contentScaleTransform.ScaleX = newScale;
                this.contentScaleTransform.ScaleY = newScale;
            }
            else
            {
                // necessary in order to immediately restore last scale after just remove contentScaleStoryboard
                this.contentScaleTransform.ScaleX = oldScale;
                this.contentScaleTransform.ScaleY = oldScale;

                var scaleStoryboard = AnimationHelper.CreateScaleStoryboard(
                    this.contentScaleTransform,
                    ZoomAnimationDurationSeconds,
                    oldScale,
                    newScale);
                this.contentScaleStoryboard = scaleStoryboard;
                this.contentScaleStoryboard.Begin(this.content, isControllable: true);

                // reset field when it's ended so panning can work
                ClientTimersSystem.AddAction(
                    delaySeconds: ZoomAnimationDurationSeconds,
                    action: () =>
                            {
                                if (ReferenceEquals(scaleStoryboard,
                                                    this.contentScaleStoryboard))
                                {
                                    // assume the animation is ended
                                    this.contentScaleStoryboard = null;
                                }
                            });
            }

            this.SetOffset(offset, isInstant: isInstant);
            this.OnZoomChanged(isByPlayersInput: true);
        }

        private static void DefaultZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PanningPanel)d;
            if (!control.isLoaded)
            {
                control.CurrentTargetZoom = control.DefaultZoom;
            }
        }

        private static void MinOrMaxZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panningPanel = (PanningPanel)d;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            panningPanel.VisibilityZoomSlider = panningPanel.MinZoom != panningPanel.MaxZoom
                                                    ? Visibility.Visible
                                                    : Visibility.Collapsed;
        }

        private static void PanningSizeChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panningPanel = (PanningPanel)d;
            panningPanel.UpdatePanningSize();
            panningPanel.Refresh();
        }

        private void AutoCalculateMinZoom()
        {
            if (!this.IsAutoCalculatingMinZoom)
            {
                return;
            }

            if (this.clipperSize == Vector2D.Zero)
            {
                // the clipper size is not calculated yet
                return;
            }

            // calculate the min zoom value to fit the panning bounds into the clipper size
            var bounds = this.panningBounds ?? new BoundsDouble(0, 0, this.panningWidth, this.panningHeight);
            var scale = Math.Max(bounds.Size.X / this.clipperSize.X,
                                 bounds.Size.Y / this.clipperSize.Y);

            if (scale < 1)
            {
                scale = 1;
            }

            this.MinZoom = Math.Min(1.0 / scale, this.MaxZoom);
        }

        private void CanvasSizeChangedHandler(object sender, SizeChangedEventArgs arg1)
        {
            this.Refresh();
        }

        private Vector2D ClampOffset(Vector2D offset)
        {
            return (-MathHelper.Clamp(-offset.X, this.offsetBounds.MinX, this.offsetBounds.MaxX),
                    -MathHelper.Clamp(-offset.Y, this.offsetBounds.MinY, this.offsetBounds.MaxY));
        }

        private void ContentWrapperMouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            var wheelRotation = e.Delta * MouseScrollWheelZoomSpeed;
            // use exponential scale https://www.gamedev.net/forums/topic/666225-equation-for-zooming/?tab=comments#comment-5213633
            var newScale = Math.Exp(Math.Log(this.CurrentTargetZoom) + wheelRotation);
            newScale = MathHelper.Clamp(newScale, this.MinZoom, this.MaxZoom);

            if (this.sliderScale != null)
            {
                this.isListeningToScaleEvents = false;
                this.sliderScale.Value = newScale;
                this.isListeningToScaleEvents = true;
            }

            this.ZoomToPoint(newScale, this.GetMouseCanvasPosition(), isInstant: false);
            //Api.Logger.Write("Current zoom: " + currentZoom.ToString("F2"), LogSeverity.Dev);
        }

        private void ContentWrapperOnMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (this.isMouseButtonDown)
            {
                return;
            }

            this.isMouseButtonDown = true;
            this.UpdateMouseHoldAbsolutePosition(this.GetMouseCanvasPosition());

            this.mouseEventsRoot.PreviewMouseLeftButtonUp += this.LayoutRootOnMouseLeftButtonUpHandler;
            this.mouseEventsRoot.MouseLeave += this.LayoutRootMouseLeaveHandler;
            this.mouseEventsRoot.PreviewMouseMove += this.LayoutRootMouseMoveHandler;

            this.MouseHold?.Invoke();
        }

        private Vector2D GetMouseCanvasPosition()
        {
            return Mouse.GetPosition(this.contentWrapper).ToVector2D();
        }

        private void LayoutRootMouseLeaveHandler(object sender, MouseEventArgs arg1)
        {
            this.ReleaseMouse();
        }

        private void LayoutRootMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!this.isMouseButtonDown)
            {
                return;
            }

            var pos = this.GetMouseCanvasPosition();
            if (!(this.contentScaleStoryboard is null))
            {
                // don't allow panning while zoom storyboard is playing
                return;
            }

            if (!this.mouseHoldAbsolutePosition.HasValue)
            {
                // perhaps the value was reset during the zooming
                this.UpdateMouseHoldAbsolutePosition(pos);
            }

            // ReSharper disable once PossibleInvalidOperationException
            var offset = pos - this.mouseHoldAbsolutePosition.Value;
            offset = this.ClampOffset(offset);
            this.SetOffset(offset, isInstant: true);
        }

        private void LayoutRootOnMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (!this.isMouseButtonDown)
            {
                return;
            }

            this.ReleaseMouse();
        }

        private void OnZoomChanged(bool isByPlayersInput)
        {
            this.ZoomChanged?.Invoke((this.CurrentTargetZoom, isByPlayersInput));
        }

        private void ReleaseMouse()
        {
            if (!this.isMouseButtonDown)
            {
                return;
            }

            this.isMouseButtonDown = false;
            this.mouseHoldAbsolutePosition = null;

            if (this.mouseEventsRoot != null)
            {
                this.mouseEventsRoot.PreviewMouseLeftButtonUp -= this.LayoutRootOnMouseLeftButtonUpHandler;
                this.mouseEventsRoot.MouseLeave -= this.LayoutRootMouseLeaveHandler;
                this.mouseEventsRoot.PreviewMouseMove -= this.LayoutRootMouseMoveHandler;
            }
        }

        private void SetOffset(Vector2D offset, bool isInstant)
        {
            this.contentTranslateStoryboard?.Remove(this.content);
            this.contentTranslateStoryboard = null;

            if (isInstant)
            {
                this.contentTranslateTransform.X = offset.X;
                this.contentTranslateTransform.BeginAnimation(TranslateTransform.XProperty, null);
                this.contentTranslateTransform.Y = offset.Y;
                this.contentTranslateTransform.BeginAnimation(TranslateTransform.YProperty, null);
            }
            else
            {
                this.contentTranslateStoryboard = AnimationHelper.CreateTransformMoveStoryboard(
                    this.contentTranslateTransform,
                    ZoomAnimationDurationSeconds,
                    fromX: this.contentTranslateTransform.X,
                    toX: offset.X,
                    fromY: this.contentTranslateTransform.Y,
                    toY: offset.Y);

                this.contentTranslateStoryboard.Begin(this.content);
            }
        }

        private void SliderScaleValueChangedHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.isListeningToScaleEvents)
            {
                return;
            }

            Vector2D? contentWrapperPoint = null;
            var canvasPoint = this.CallbackGetSliderZoomCanvasPosition?.Invoke();
            if (canvasPoint.HasValue)
            {
                var screenPoint = this.content.PointToScreen(new Point(canvasPoint.Value.X,
                                                                       canvasPoint.Value.Y));
                contentWrapperPoint = this.contentWrapper.PointFromScreen(screenPoint).ToVector2D();
            }

            if (contentWrapperPoint == null)
            {
                // zoom to center of the screen if no custom zoom position provided
                contentWrapperPoint = (this.contentWrapper.ActualWidth / 2,
                                       this.contentWrapper.ActualHeight / 2);
            }

            this.ZoomToPoint(this.sliderScale.Value, contentWrapperPoint.Value, isInstant: false);
        }

        private void UpdateBounds()
        {
            if (!this.isLoaded)
            {
                return;
            }

            var visibleBounds = this.panningBounds
                                ?? new BoundsDouble(0, 0, this.panningWidth, this.panningHeight);

            visibleBounds = new BoundsDouble(
                offset: visibleBounds.Offset * this.CurrentTargetZoom,
                size: visibleBounds.Size * this.CurrentTargetZoom);

            // Now we have the canvas content visible bounds.
            // Please note: clipper size is the observable size of the content.
            // Let's calculate the offset bounds.

            // ReSharper disable once LocalVariableHidesMember
            var offsetBounds = new BoundsDouble(
                offset: visibleBounds.Offset,
                size: (Math.Max(0, visibleBounds.Size.X - this.clipperSize.X),
                       Math.Max(0, visibleBounds.Size.Y - this.clipperSize.Y)));

            // calculate extra padding
            Vector2D padding = (Math.Max(0, this.clipperSize.X - visibleBounds.Size.X),
                                Math.Max(0, this.clipperSize.Y - visibleBounds.Size.Y));

            // center bounds (so that if content is small it will be centered)
            offsetBounds = new BoundsDouble(
                offset: visibleBounds.Offset - padding / 2,
                size: (Math.Max(0, offsetBounds.Size.X - padding.X / 2),
                       Math.Max(0, offsetBounds.Size.Y - padding.Y / 2)));

            this.offsetBounds = offsetBounds;
        }

        private void UpdateMouseHoldAbsolutePosition(Vector2D currentMouseCanvasPosition)
        {
            this.mouseHoldAbsolutePosition = (currentMouseCanvasPosition.X - this.contentTranslateTransform.X,
                                              currentMouseCanvasPosition.Y - this.contentTranslateTransform.Y);
        }

        private void UpdatePanningSize()
        {
            if (!this.isLoaded)
            {
                return;
            }

            var width = this.PanningWidth;
            var height = this.PanningHeight;

            if (width == 0
                || double.IsNaN(width)
                || double.IsInfinity(width)
                || height == 0
                || double.IsNaN(height)
                || double.IsInfinity(height))
            {
                Api.Logger.Error("Width and/or height is/are incorrect for " + this.Name);
                return;
            }

            this.panningWidth = width;
            this.panningHeight = height;

            this.AutoCalculateMinZoom();
        }
    }
}