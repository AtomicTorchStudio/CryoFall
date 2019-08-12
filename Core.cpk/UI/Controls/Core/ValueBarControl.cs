namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ValueBarControl : BaseContentControl
    {
        // Allow to interpolate if the fraction difference between
        // the current interpolation value and actual this.Value is less than this value
        // (for example, 0.5 means 50% difference max - if it's bigger the interpolation will be skipped).
        private const double DefaultInterpolationMaxDifferenceFraction = 0.5;

        private const double DefaultInterpolationMinDifferenceFraction = 0.0001;

        private const double DefaultValueInterpolationRate = 20;

        public static readonly DependencyProperty InterpolationMinDifferenceFractionProperty =
            DependencyProperty.Register(nameof(InterpolationMinDifferenceFraction),
                                        typeof(double),
                                        typeof(ValueBarControl),
                                        new PropertyMetadata(DefaultInterpolationMinDifferenceFraction));

        public static readonly DependencyProperty InterpolationMaxDifferenceFractionProperty =
            DependencyProperty.Register(nameof(InterpolationMaxDifferenceFraction),
                                        typeof(double),
                                        typeof(ValueBarControl),
                                        new PropertyMetadata(DefaultInterpolationMaxDifferenceFraction));

        public static readonly DependencyProperty BarBrushProperty =
            DependencyProperty.Register(
                nameof(BarBrush),
                typeof(Brush),
                typeof(ValueBarControl),
                new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty DefaultContentTemplateProperty =
            DependencyProperty.Register(
                nameof(DefaultContentTemplate),
                typeof(ControlTemplate),
                typeof(ValueBarControl),
                new PropertyMetadata(default(ControlTemplate)));

        public static readonly DependencyProperty IsReversedBarProperty =
            DependencyProperty.Register(
                nameof(IsReversedBar),
                typeof(bool),
                typeof(ValueBarControl),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsDisplayBarProperty = DependencyProperty.Register(
            nameof(IsDisplayBar),
            typeof(bool),
            typeof(ValueBarControl),
            new FrameworkPropertyMetadata(true, DisplayPropertyChangedHandler));

        public static readonly DependencyProperty IsDisplayLabelProperty = DependencyProperty.Register(
            nameof(IsDisplayLabel),
            typeof(bool),
            typeof(ValueBarControl),
            new FrameworkPropertyMetadata(true, DisplayPropertyChangedHandler));

        public static readonly DependencyProperty IsDisplayPercentsProperty = DependencyProperty.Register(
            nameof(IsDisplayPercents),
            typeof(bool),
            typeof(ValueBarControl),
            new FrameworkPropertyMetadata(true, DisplayPropertyChangedHandler));

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            nameof(MaxValue),
            typeof(double),
            typeof(ValueBarControl),
            new FrameworkPropertyMetadata(1f, MaxValuePropertyChangedHandler));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(ValueBarControl),
            new FrameworkPropertyMetadata(1f, ValuePropertyChangedHandler));

        public static readonly DependencyProperty CurrentValueBarWidthProperty =
            DependencyProperty.Register(
                nameof(CurrentValueBarWidth),
                typeof(double),
                typeof(ValueBarControl),
                new PropertyMetadata(default(double)));

        public static readonly DependencyProperty PositionsCountProperty =
            DependencyProperty.Register(
                nameof(PositionsCount),
                typeof(uint?),
                typeof(ValueBarControl),
                new PropertyMetadata(null, PositionsCountPropertyChanged));

        public static readonly DependencyProperty IsDisplayTooltipProperty =
            DependencyProperty.Register(
                nameof(IsDisplayTooltip),
                typeof(bool),
                typeof(ValueBarControl),
                new PropertyMetadata(defaultValue: false,
                                     propertyChangedCallback: IsDisplayTooltipPropertyChanged));

        public static readonly DependencyProperty TooltipFormatProperty =
            DependencyProperty.Register(
                nameof(TooltipFormat),
                typeof(string),
                typeof(ValueBarControl),
                new PropertyMetadata(default(string), TooltipFormatPropertyChanged));

        public static readonly DependencyProperty LabelFormatProperty =
            DependencyProperty.Register(
                nameof(LabelFormat),
                typeof(string),
                typeof(ValueBarControl),
                new PropertyMetadata(default(string), LabelFormatPropertyChanged));

        public static readonly DependencyProperty IsValueInterpolatedProperty =
            DependencyProperty.Register(nameof(IsValueInterpolated),
                                        typeof(bool),
                                        typeof(ValueBarControl),
                                        new PropertyMetadata(defaultValue: true,
                                                             propertyChangedCallback:
                                                             IsValueInterpolatedPropertyChanged));

        public static readonly DependencyProperty ValueInterpolationRateProperty =
            DependencyProperty.Register(nameof(ValueInterpolationRate),
                                        typeof(double),
                                        typeof(ValueBarControl),
                                        new PropertyMetadata(DefaultValueInterpolationRate));

        private double barHeightMax;

        private double barWidthMax;

        private Border border;

        private uint? cachedPositionsCount;

        private StreamGeometry clipGeometry;

        private FrameworkElement contentControl;

        private double interpolatedValue;

        private bool isDisplayBar;

        private bool isDisplayLabel;

        private bool isDisplayPercents;

        private bool isFrameSet;

        private bool isTooltipEnabled;

        private bool isValueInterpolated;

        private string labelFormat;

        private int? lastValue;

        private double lastValueMax;

        private TextBlock textBlockValueDisplay;

        private string tooltipFormat;

        private TextBlock tooltipTextBlock;

        static ValueBarControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ValueBarControl),
                new FrameworkPropertyMetadata(typeof(ValueBarControl)));
        }

        public ValueBarControl()
        {
        }

        public Brush BarBrush
        {
            get => (Brush)this.GetValue(BarBrushProperty);
            set => this.SetValue(BarBrushProperty, value);
        }

        public double CurrentValueBarWidth
        {
            get => (double)this.GetValue(CurrentValueBarWidthProperty);
            set => this.SetValue(CurrentValueBarWidthProperty, value);
        }

        public ControlTemplate DefaultContentTemplate
        {
            get => (ControlTemplate)this.GetValue(DefaultContentTemplateProperty);
            set => this.SetValue(DefaultContentTemplateProperty, value);
        }

        public double InterpolationMaxDifferenceFraction
        {
            get => (double)this.GetValue(InterpolationMaxDifferenceFractionProperty);
            set => this.SetValue(InterpolationMaxDifferenceFractionProperty, value);
        }

        public double InterpolationMinDifferenceFraction
        {
            get => (double)this.GetValue(InterpolationMinDifferenceFractionProperty);
            set => this.SetValue(InterpolationMinDifferenceFractionProperty, value);
        }

        public bool IsDisplayBar
        {
            get => (bool)this.GetValue(IsDisplayBarProperty);
            set => this.SetValue(IsDisplayBarProperty, value);
        }

        public bool IsDisplayLabel
        {
            get => (bool)this.GetValue(IsDisplayLabelProperty);
            set => this.SetValue(IsDisplayLabelProperty, value);
        }

        public bool IsDisplayPercents
        {
            get => (bool)this.GetValue(IsDisplayPercentsProperty);
            set => this.SetValue(IsDisplayPercentsProperty, value);
        }

        public bool IsDisplayTooltip
        {
            get => (bool)this.GetValue(IsDisplayTooltipProperty);
            set => this.SetValue(IsDisplayTooltipProperty, value);
        }

        public bool IsReversedBar
        {
            get => (bool)this.GetValue(IsReversedBarProperty);
            set => this.SetValue(IsReversedBarProperty, value);
        }

        public bool IsValueInterpolated
        {
            get => (bool)this.GetValue(IsValueInterpolatedProperty);
            set => this.SetValue(IsValueInterpolatedProperty, value);
        }

        public string LabelFormat
        {
            get => (string)this.GetValue(LabelFormatProperty);
            set => this.SetValue(LabelFormatProperty, value);
        }

        public double MaxValue
        {
            get => (double)this.GetValue(MaxValueProperty);
            set => this.SetValue(MaxValueProperty, value);
        }

        public uint? PositionsCount
        {
            get => (uint?)this.GetValue(PositionsCountProperty);
            set => this.SetValue(PositionsCountProperty, value);
        }

        public string TooltipFormat
        {
            get => (string)this.GetValue(TooltipFormatProperty);
            set => this.SetValue(TooltipFormatProperty, value);
        }

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        public double ValueInterpolationRate
        {
            get => (double)this.GetValue(ValueInterpolationRateProperty);
            set => this.SetValue(ValueInterpolationRateProperty, value);
        }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            var root = templateRoot.GetByName<FrameworkElement>("LayoutRoot");
            this.border = (Border)root.FindName("Border");
            this.textBlockValueDisplay = (TextBlock)root.FindName("TextBlockValueDisplay");
        }

        protected override void OnLoaded()
        {
            this.isValueInterpolated = this.IsValueInterpolated;

            this.contentControl = this.Content as FrameworkElement;
            if (this.contentControl == null)
            {
                if (this.DefaultContentTemplate == null)
                {
                    Api.Logger.Error(
                        "Content not set for value bar control with Name="
                        + this.Name
                        + " and no DefaultContentTemplate provided.");
                }

                // create content with template
                this.contentControl = new Control()
                {
                    Template = this.DefaultContentTemplate,
                    DataContext = this
                };
                this.Content = this.contentControl;
            }
            else if (this.contentControl.Margin != new Thickness(0, 0, 0, 0))
            {
                Api.Logger.Error(
                    $"Found a {nameof(ValueBarControl)} with content Margin != 0: {this.Name} at {(this.Parent as FrameworkElement)?.Name}");
            }

            this.clipGeometry = new StreamGeometry();
            this.clipGeometry.FillRule = FillRule.EvenOdd;
            this.contentControl.Clip = this.clipGeometry;

            this.interpolatedValue = this.Value;
            this.UpdateFrame();

            this.SizeChanged += this.SizeChangedHandler;

            if (this.isValueInterpolated)
            {
                ClientUpdateHelper.UpdateCallback += this.UpdateInterpolatedValue;
            }
        }

        protected override void OnUnloaded()
        {
            this.SizeChanged -= this.SizeChangedHandler;

            if (this.isValueInterpolated)
            {
                ClientUpdateHelper.UpdateCallback -= this.UpdateInterpolatedValue;
            }
        }

        private static void DisplayPropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ValueBarControl)d;
            if (control.isLoaded)
            {
                control.UpdateFrame();
            }
        }

        private static void IsDisplayTooltipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ValueBarControl)d).IsDisplayTooltipPropertyChanged();
        }

        private static void IsValueInterpolatedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ValueBarControl)d;
            if (!control.isLoaded)
            {
                return;
            }

            var isValueInterpolated = (bool)e.NewValue;
            control.isValueInterpolated = isValueInterpolated;
            if (isValueInterpolated)
            {
                ClientUpdateHelper.UpdateCallback += control.UpdateInterpolatedValue;
            }
            else
            {
                ClientUpdateHelper.UpdateCallback -= control.UpdateInterpolatedValue;
            }
        }

        private static void LabelFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ValueBarControl)d).labelFormat = (string)e.NewValue;
        }

        private static void MaxValuePropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ValueBarControl)d;
            control.UpdateCurrentTooltipText();
            control.RefreshBar();
        }

        private static void PositionsCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var valueBarControl = (ValueBarControl)d;
            valueBarControl.cachedPositionsCount = (uint?)e.NewValue;
            valueBarControl.RefreshBar();
        }

        private static void TooltipFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ValueBarControl)d).tooltipFormat = (string)e.NewValue;
        }

        private static void ValuePropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ValueBarControl)d;
            control.UpdateCurrentTooltipText();

            if (control.IsValueInterpolated)
            {
                // it will be updated automatically in this or the next frame
                //control.UpdateInterpolatedValue();
            }
            else
            {
                control.RefreshBar();
            }
        }

        private void IsDisplayTooltipPropertyChanged()
        {
            if (this.isTooltipEnabled)
            {
                this.MouseEnter -= this.MouseEnterHandler;
                this.MouseLeave -= this.MouseLeaveHandler;
            }

            // cache value
            this.isTooltipEnabled = this.IsDisplayTooltip;

            if (this.isTooltipEnabled)
            {
                this.MouseEnter += this.MouseEnterHandler;
                this.MouseLeave += this.MouseLeaveHandler;
            }
        }

        private void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            this.tooltipTextBlock = new TextBlock();
            this.UpdateCurrentTooltipText();
            ToolTipServiceExtend.SetToolTip(this, this.tooltipTextBlock);
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            this.tooltipTextBlock = null;
            ToolTipServiceExtend.SetToolTip(this, null);
        }

        private void RefreshBar()
        {
            if (!this.isLoaded
                || !this.isFrameSet)
            {
                return;
            }

            var value = this.isValueInterpolated ? this.interpolatedValue : this.Value;
            var maxValue = this.MaxValue;

            if (this.IsReversedBar)
            {
                value = maxValue - value;
            }

            if (this.isDisplayBar)
            {
                // position fraction (in [0;1] range)
                var fraction = value / maxValue;

                if (this.cachedPositionsCount.HasValue)
                {
                    // fix position to closest lower value
                    fraction *= this.cachedPositionsCount.Value;
                    fraction = (int)fraction / (double)this.cachedPositionsCount.Value;
                }

                if (double.IsNaN(fraction)
                    || double.IsInfinity(fraction))
                {
                    fraction = 0;
                }

                if (this.barWidthMax == 0
                    || this.barHeightMax == 0)
                {
                    Api.Logger.Error(
                        string.Format(
                            "{3}: wrong values in UpdateDisplay(): barWidthMax={0:R}, barHeightMax={1:R}, divide={2:R}",
                            this.barWidthMax,
                            this.barHeightMax,
                            fraction,
                            this.Name));
                }

                var currentValueBarWidth = fraction * this.barWidthMax;
                this.CurrentValueBarWidth = currentValueBarWidth;

                using var ctx = this.clipGeometry.Open();
                ctx.BeginFigure(new Point(0,               0), true, true);
                ctx.LineTo(new Point(currentValueBarWidth, 0),                 false, false);
                ctx.LineTo(new Point(currentValueBarWidth, this.barHeightMax), false, false);
                ctx.LineTo(new Point(0,                    this.barHeightMax), false, false);
            }

            if (!this.isDisplayLabel
                || this.textBlockValueDisplay == null)
            {
                // do not update text
                return;
            }

            // update text
            if (this.isDisplayPercents)
            {
                // display percent value
                var percentValue = value / maxValue;
                if (double.IsNaN(percentValue)
                    || double.IsInfinity(percentValue))
                {
                    percentValue = 0;
                }

                var v = (int)Math.Round(percentValue * 100.0,
                                        MidpointRounding.AwayFromZero);
                if (this.lastValue != v)
                {
                    this.lastValue = v;
                    this.textBlockValueDisplay.Text = this.lastValue + "%";
                }
            }
            else
            {
                // update text value in format "current/max"
                var v = (int)Math.Round(value, 
                                        MidpointRounding.AwayFromZero);
                if (this.lastValue == v
                    && this.lastValueMax == maxValue)
                {
                    // no need to update
                    return;
                }

                this.lastValue = v;
                this.lastValueMax = maxValue;
                this.textBlockValueDisplay.Text =
                    this.labelFormat != null
                        ? string.Format(this.labelFormat, value, maxValue)
                        : this.lastValue + "/" + (int)Math.Round(maxValue,
                                                                 MidpointRounding.AwayFromZero);
            }
        }

        private void SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            this.UpdateFrame();
        }

        private void UpdateCurrentTooltipText()
        {
            if (this.tooltipTextBlock == null)
            {
                // no current tooltip
                return;
            }

            var value = this.Value;
            var maxValue = this.MaxValue;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            var valuePercents = maxValue != 0.0 
                                    ? (int)Math.Round(100.0 * value / maxValue, 
                                                      MidpointRounding.AwayFromZero) 
                                    : 0;

            if (!string.IsNullOrEmpty(this.tooltipFormat))
            {
                this.tooltipTextBlock.Text =
                    string.Format(this.tooltipFormat, valuePercents, value, maxValue);
            }
            else
            {
                this.tooltipTextBlock.Text = valuePercents + "%";
            }
        }

        private void UpdateFrame()
        {
            if (!this.isLoaded)
            {
                return;
            }

            if (this.contentControl == null)
            {
                throw new Exception("Content control not found for ValueBarControl with Name=" + this.Name);
            }

            var maxWidth = this.MaxWidth;
            if (maxWidth <= 0
                || double.IsNaN(maxWidth)
                || double.IsInfinity(maxWidth))
            {
                maxWidth = this.ActualWidth;
            }

            var maxHeight = this.MaxHeight;
            if (maxHeight <= 0
                || double.IsNaN(maxHeight)
                || double.IsInfinity(maxHeight))
            {
                maxHeight = this.ActualHeight;
            }

            this.barWidthMax = maxWidth - this.Padding.Left - this.Padding.Right;
            this.barHeightMax = maxHeight - this.Padding.Top - this.Padding.Bottom;

            if (this.barWidthMax < 0)
            {
                this.barWidthMax = 0;
            }

            if (this.barHeightMax < 0)
            {
                this.barHeightMax = 0;
            }

            if (double.IsNaN(this.barWidthMax)
                || double.IsNaN(this.barHeightMax)
                || double.IsInfinity(this.barWidthMax)
                || double.IsInfinity(this.barHeightMax)
                || this.barWidthMax == 0
                || this.barHeightMax == 0)
            {
                //Api.Logger.WriteError(
                //	string.Format(
                //		"{2}: wrong frame size: barWidthMax={0:R}, barHeightMax={1:R}",
                //		this.barWidthMax,
                //		this.barHeightMax,
                //		this.Name));
                return;
            }

            this.contentControl.Width = this.barWidthMax;
            this.contentControl.Height = this.barHeightMax;

            this.isDisplayLabel = this.IsDisplayLabel;
            this.isDisplayBar = this.IsDisplayBar;
            this.isDisplayPercents = this.IsDisplayPercents;

            if (this.textBlockValueDisplay != null)
            {
                this.textBlockValueDisplay.Visibility = this.isDisplayLabel ? Visibility.Visible : Visibility.Collapsed;
            }

            var barVisibility = this.isDisplayBar ? Visibility.Visible : Visibility.Collapsed;
            this.contentControl.Visibility = barVisibility;
            if (this.border != null)
            {
                this.border.Visibility = barVisibility;
            }

            // Scripting.Logger.Write("Clip max size: " + this.barWidthMax + "; " + this.barHeightMax);
            this.isFrameSet = true;
            this.RefreshBar();
        }

        /// <summary>
        /// Interpolate displayed bar value every frame (if the value interpolation is enabled).
        /// </summary>
        private void UpdateInterpolatedValue()
        {
            if (!this.isValueInterpolated)
            {
                return;
            }

            var actualValue = this.Value;
            if (this.interpolatedValue == actualValue)
            {
                return;
            }

            var maxValue = this.MaxValue;

            //if (maxValue == 100)
            //{
            //    Api.Logger.WriteDev("interpolating: "
            //                        + actualValue.ToString("F2")
            //                        + " -> "
            //                        + this.interpolatedValue.ToString("F2"));
            //}

            var differenceFraction = Math.Abs(actualValue - this.interpolatedValue) / maxValue;

            if (differenceFraction < this.InterpolationMaxDifferenceFraction
                && differenceFraction > this.InterpolationMinDifferenceFraction)
            {
                // the difference is small enough to interpolate between the values
                this.interpolatedValue = MathHelper.LerpWithDeltaTime(
                    this.interpolatedValue,
                    actualValue,
                    Api.Client.Core.DeltaTime,
                    rate: this.ValueInterpolationRate);

                if (Math.Abs(this.interpolatedValue - actualValue) < double.Epsilon)
                {
                    this.interpolatedValue = actualValue;
                }
            }
            else
            {
                // the difference is too large (interpolation will look wrong!)
                // OR difference is too small
                this.interpolatedValue = actualValue;
            }

            this.RefreshBar();
        }
    }
}