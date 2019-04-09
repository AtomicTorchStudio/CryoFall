namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class PanningPanel : BaseItemsControl
    {
        public const double ZoomingFuncPower = 2;
        // public const double ZoomAnimationDurationSeconds = 0.0667;

        public static readonly DependencyProperty CenterCommandProperty = DependencyProperty.Register(
            nameof(CenterCommand),
            typeof(BaseCommand),
            typeof(PanningPanel),
            new PropertyMetadata(default(BaseCommand)));

        public static readonly DependencyProperty DefaultZoomProperty = DependencyProperty.Register(
            nameof(DefaultZoom),
            typeof(double),
            typeof(PanningPanel),
            new PropertyMetadata(1d));

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
                                        new PropertyMetadata(default(bool)));

        private ClickHandlerHelper clickHandlerHelper;

        private Vector2D clipperSize;

        private RectangleGeometry clippingRect;

        private Canvas content;

        private UIElement contentItems;

        private ScaleTransform contentScaleTransform;

        private TranslateTransform contentTranslateTransform;

        private Canvas contentWrapper;

        private double currentZoom = 1;

        private bool isInitialized;

        private bool isListeningToScaleEvents = true;

        private bool isMouseButtonDown;

        private Panel mouseEventsRoot;

        private Vector2D mouseHoldPosition;

        private Panel mouseInputRoot;

        private BoundsDouble offsetBounds;

        private BoundsDouble? panningBounds;

        private double panningHeight;

        private double panningWidth;

        private double realScale = 1;

        private Vector2D? requireCenterOnPoint;

        private Slider sliderScale;

        static PanningPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PanningPanel),
                new FrameworkPropertyMetadata(typeof(PanningPanel)));
        }

        public PanningPanel()
        {
        }

        public event Action<PanningPanel, MouseEventArgs> MouseLeftButtonClick;

        public event Action<double> ZoomChanged;

        public BaseCommand CenterCommand
        {
            get => (BaseCommand)this.GetValue(CenterCommandProperty);
            set => this.SetValue(CenterCommandProperty, value);
        }

        public Vector2D CurrentOffset
        {
            get
            {
                if (this.contentTranslateTransform == null)
                {
                    return Vector2D.Zero;
                }

                return (this.contentTranslateTransform.X, this.contentTranslateTransform.Y);
            }
            set
            {
                if (this.contentTranslateTransform == null)
                {
                    return;
                }

                this.contentTranslateTransform.X = value.X;
                this.contentTranslateTransform.Y = value.Y;
            }
        }

        public double CurrentZoom => this.currentZoom;

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
            Vector2D offset = ((this.clipperSize.X - point.X * 2d * scale) / 2d,
                               (this.clipperSize.Y - point.Y * 2d * scale) / 2d);

            offset = this.ClampOffset(offset);
            this.SetOffset(offset);

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
            newScale = MathHelper.Clamp(newScale, this.MinZoom, this.MaxZoom);
            this.realScale = Math.Pow(newScale, 1 / ZoomingFuncPower);
            this.currentZoom = newScale;

            this.contentScaleTransform.ScaleX = newScale;
            this.contentScaleTransform.ScaleY = newScale;

            if (this.isLoaded)
            {
                this.UpdateBounds();

                this.isListeningToScaleEvents = false;
                this.sliderScale.Value = newScale;
                this.isListeningToScaleEvents = true;
            }

            this.OnZoomChanged();
        }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            var layoutRoot = templateRoot.GetByName<Grid>("LayoutRoot");

            this.contentWrapper = layoutRoot.GetByName<Canvas>("CanvasContentWrapper");
            this.content = layoutRoot.GetByName<Canvas>("CanvasContent");
            this.contentItems = layoutRoot.GetByName<Canvas>("CanvasContentItems");

            this.mouseEventsRoot = IsDesignTime ? layoutRoot : Api.Client.UI.LayoutRoot;
            this.mouseInputRoot = this.contentWrapper;

            this.contentScaleTransform = new ScaleTransform();
            this.contentTranslateTransform = new TranslateTransform();

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(this.contentScaleTransform);
            transformGroup.Children.Add(this.contentTranslateTransform);
            this.content.RenderTransform = transformGroup;

            this.clippingRect = new RectangleGeometry();
            this.contentWrapper.Clip = this.clippingRect;
            this.sliderScale = layoutRoot.GetByName<Slider>("SliderScale");

            this.CenterCommand = new ActionCommand(this.CenterCommandAction);
        }

        protected override void OnLoaded()
        {
            this.UpdatePanningSize();

            if (!this.isInitialized)
            {
                this.SetZoom(this.DefaultZoom);
                this.isInitialized = true;
            }
            else
            {
                this.SetZoom(this.currentZoom);
            }

            if (!IsDesignTime)
            {
                // subscribe to drag/zoom mouse events
                this.contentWrapper.PreviewMouseLeftButtonDown += this.ContentWrapperOnMouseLeftButtonDownHandler;
                this.contentWrapper.MouseWheel += this.ContentWrapperMouseWheelHandler;
                this.sliderScale.ValueChanged += this.SliderScaleValueChangedHandler;
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
            this.sliderScale.ValueChanged -= this.SliderScaleValueChangedHandler;
            this.SizeChanged -= this.CanvasSizeChangedHandler;

            this.clickHandlerHelper.Dispose();
            this.clickHandlerHelper = null;
        }

        /// <summary>
        /// Zoom to screen point.
        /// </summary>
        protected void ZoomToPoint(double newScale, Vector2D screenPoint, bool isInstant)
        {
            if (!this.isLoaded)
            {
                return;
            }

            this.currentZoom = newScale;

            var oldScale = this.contentScaleTransform.ScaleX;

            Vector2D currentOffset = (this.contentTranslateTransform.X,
                                      this.contentTranslateTransform.Y);

            // fix on cursor to avoid moving while scaling
            var delta = ((newScale - oldScale) * (currentOffset.X - screenPoint.X) / oldScale,
                         (newScale - oldScale) * (currentOffset.Y - screenPoint.Y) / oldScale);

            this.UpdateBounds();
            var offset = currentOffset + delta;
            offset = this.ClampOffset(offset);

            this.mouseHoldPosition = (
                                         this.mouseHoldPosition.X + currentOffset.X - offset.X,
                                         this.mouseHoldPosition.Y + currentOffset.Y - offset.Y);

            isInstant = true;

            if (isInstant)
            {
                this.contentScaleTransform.ScaleX = newScale;
                this.contentScaleTransform.ScaleY = newScale;

                //Api.Logger.WriteDev(
                //	string.Format(
                //		"Zooming to x{0} from x{1}|y{2} ({3}|{4})",
                //		newScale,
                //		this.canvasContentScaleTransform.GetValue(ScaleTransform.ScaleXProperty),
                //		this.canvasContentScaleTransform.GetValue(ScaleTransform.ScaleYProperty),
                //		this.canvasContentScaleTransform.ScaleX,
                //		this.canvasContentScaleTransform.ScaleY));

                this.SetOffset(offset);
            }
            //else
            //{
            //	this.scaleStoryboard = AnimationHelper.CreateScaleStoryboard(
            //		this.scaleTransform,
            //		ZoomAnimationDurationSeconds,
            //		oldScale,
            //		newScale);

            //	this.translateStoryboard = AnimationHelper.CreateTransformMoveStoryboard(
            //		this.translateTransform,
            //		ZoomAnimationDurationSeconds,
            //		currentX,
            //		newX,
            //		currentY,
            //		newY);

            //	this.scaleStoryboard.Begin(this.canvasContent);
            //	this.translateStoryboard.Begin(this.canvasContent);
            //}

            this.OnZoomChanged();
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

        private void CenterCommandAction()
        {
            this.SetZoom(this.DefaultZoom);
            this.CenterCanvasContent();
        }

        private Vector2D ClampOffset(Vector2D offset)
        {
            return (
                       -MathHelper.Clamp(-offset.X, this.offsetBounds.MinX, this.offsetBounds.MaxX),
                       -MathHelper.Clamp(-offset.Y, this.offsetBounds.MinY, this.offsetBounds.MaxY));
        }

        private void ContentWrapperMouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            var wheelRotation = e.Delta * 0.001d;
            this.realScale = MathHelper.Clamp(
                this.realScale + wheelRotation,
                this.MinZoom,
                Math.Pow(this.MaxZoom, 1 / ZoomingFuncPower));

            var newScale = ZoomingFuncPower != 1d && this.realScale > 1
                               ? Math.Pow(this.realScale, ZoomingFuncPower)
                               : this.realScale;

            this.isListeningToScaleEvents = false;
            this.sliderScale.Value = newScale;
            this.isListeningToScaleEvents = true;

            this.ZoomToPoint(newScale, this.GetMouseCanvasPosition(e), isInstant: false);
            //Api.Logger.Write("Current zoom: " + currentZoom.ToString("F2"), LogSeverity.Dev);
        }

        private void ContentWrapperOnMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (this.isMouseButtonDown)
            {
                return;
            }

            this.isMouseButtonDown = true;
            var pos = this.GetMouseCanvasPosition(e);
            this.mouseHoldPosition = (pos.X - this.contentTranslateTransform.X,
                                      pos.Y - this.contentTranslateTransform.Y);

            this.mouseEventsRoot.PreviewMouseLeftButtonUp += this.LayoutRootOnMouseLeftButtonUpHandler;
            this.mouseEventsRoot.MouseLeave += this.LayoutRootMouseLeaveHandler;
            this.mouseEventsRoot.PreviewMouseMove += this.LayoutRootMouseMoveHandler;
        }

        private Vector2D GetMouseCanvasPosition(MouseEventArgs e)
        {
            return e.GetPosition(this.contentWrapper).ToVector2D();
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

            //if (this.scaleStoryboard != null
            //    && this.scaleStoryboard.IsPlaying(this.canvasContent))
            //{
            //	return;
            //}

            var pos = this.GetMouseCanvasPosition(e);
            var offset = pos - this.mouseHoldPosition;
            offset = this.ClampOffset(offset);
            this.SetOffset(offset);

            //Api.Logger.WriteDev(
            //	string.Format(
            //		"Moving to x={0} y={1} (x={2}|y={3})",
            //		newX,
            //		newY,
            //		this.canvasContentTranslateTransform.X,
            //		this.canvasContentTranslateTransform.Y));
        }

        private void LayoutRootOnMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (!this.isMouseButtonDown)
            {
                return;
            }

            this.ReleaseMouse();
        }

        private void OnZoomChanged()
        {
            this.ZoomChanged?.Invoke(this.currentZoom);
        }

        private void ReleaseMouse()
        {
            if (!this.isMouseButtonDown)
            {
                return;
            }

            this.isMouseButtonDown = false;

            if (this.mouseEventsRoot != null)
            {
                this.mouseEventsRoot.PreviewMouseLeftButtonUp -= this.LayoutRootOnMouseLeftButtonUpHandler;
                this.mouseEventsRoot.MouseLeave -= this.LayoutRootMouseLeaveHandler;
                this.mouseEventsRoot.PreviewMouseMove -= this.LayoutRootMouseMoveHandler;
            }
        }

        private void SetOffset(Vector2D offset)
        {
            this.contentTranslateTransform.ClearValue(TranslateTransform.XProperty);
            this.contentTranslateTransform.ClearValue(TranslateTransform.YProperty);
            this.contentTranslateTransform.X = offset.X;
            this.contentTranslateTransform.Y = offset.Y;
        }

        private void SliderScaleValueChangedHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.isListeningToScaleEvents)
            {
                return;
            }

            this.ZoomToPoint(
                e.NewValue,
                (this.contentWrapper.ActualWidth / 2, this.contentWrapper.ActualHeight / 2),
                isInstant: false);
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
                offset: visibleBounds.Offset * this.currentZoom,
                size: visibleBounds.Size * this.currentZoom);

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