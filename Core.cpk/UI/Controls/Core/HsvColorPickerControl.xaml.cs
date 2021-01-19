namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    /// <summary>
    /// Based on Noesis Color Picker sample updated by @stonstad
    /// https://www.noesisengine.com/forums/viewtopic.php?t=1905#
    /// </summary>
    public partial class HsvColorPickerControl : BaseUserControl
    {
        public static readonly DependencyProperty SelectedColorProperty
            = DependencyProperty.Register("SelectedColor",
                                          typeof(Color),
                                          typeof(HsvColorPickerControl),
                                          new FrameworkPropertyMetadata(
                                              Colors.White,
                                              FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                              ColorPropertyChanged));

        private GradientStop gradientStop;

        private Grid hsGrid;

        private double hue;

        private TranslateTransform pickerTransform;

        private double saturation;

        private Slider spectrum;

        private TextBlock textBlock;

        private double value;

        public event EventHandler<ColorEventArgs> ColorChanged;

        public Color SelectedColor
        {
            get => (Color)this.GetValue(SelectedColorProperty);
            set => this.SetValue(SelectedColorProperty, value);
        }

        protected override void OnLoaded()
        {
            var root = (FrameworkElement)this.Content;

            this.spectrum = root.GetByName<Slider>("Slider");
            this.hsGrid = root.GetByName<Grid>("HS");
            this.pickerTransform = root.GetByName<TranslateTransform>("PickerTransform");
            this.textBlock = root.GetByName<TextBlock>("Text");
            this.gradientStop = root.GetByName<GradientStop>("Stop");

            this.spectrum.ValueChanged += this.OnSpectrumChange;
            this.hsGrid.MouseLeftButtonDown += this.OnMouseLeftButtonDown;
            this.hsGrid.MouseLeftButtonUp += this.OnMouseLeftButtonUp;
            this.hsGrid.MouseMove += this.OnMouseMove;
            this.hsGrid.SizeChanged += this.OnSizeChanged;

            this.SetHsvFromColor(this.SelectedColor);
            this.Update();
        }

        protected override void OnUnloaded()
        {
            this.spectrum.ValueChanged -= this.OnSpectrumChange;
            this.hsGrid.MouseLeftButtonDown -= this.OnMouseLeftButtonDown;
            this.hsGrid.MouseLeftButtonUp -= this.OnMouseLeftButtonUp;
            this.hsGrid.MouseMove -= this.OnMouseMove;
            this.hsGrid.SizeChanged -= this.OnSizeChanged;
        }

        private static Color ColorFromScRgb(double a, double r, double g, double b)
        {
            return Color.FromArgb((byte)(a * byte.MaxValue),
                                  (byte)(r * byte.MaxValue),
                                  (byte)(g * byte.MaxValue),
                                  (byte)(b * byte.MaxValue));
        }

        private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HsvColorPickerControl)d;
            if (!control.isLoaded)
            {
                return;
            }

            var newColor = e.NewValue is Color color ? color : default;
            Api.Logger.Dev("Color property changed: " + newColor);
            control.SetHsvFromColor(newColor);
            control.Update();
        }

        private static Color HsvToColor(double hue, double saturation, double value)
        {
            var chroma = value * saturation;
            var hueTag = hue % 360 / 60;
            var x = chroma * (1 - Math.Abs(hueTag % 2.0f - 1));
            var m = value - chroma;

            return (int)hueTag switch
            {
                0 => ColorFromScRgb(1, chroma + m, x + m,      m),
                1 => ColorFromScRgb(1, x + m,      chroma + m, m),
                2 => ColorFromScRgb(1, m,          chroma + m, x + m),
                3 => ColorFromScRgb(1, m,          x + m,      chroma + m),
                4 => ColorFromScRgb(1, x + m,      m,          chroma + m),
                _ => ColorFromScRgb(1, chroma + m, m,          x + m)
            };
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            this.Focus();
            this.hsGrid.CaptureMouse();
            var point = this.hsGrid.PointFromScreen(args.GetPosition(null));
            this.UpdatePickerPosition(point);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            this.hsGrid.ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (!this.hsGrid.IsMouseCaptured)
            {
                return;
            }

            var point = this.hsGrid.PointFromScreen(args.GetPosition(null));
            this.UpdatePickerPosition(point);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            var size = args.NewSize;
            this.pickerTransform.X = this.value * size.Width - 0.5f * size.Width;
            this.pickerTransform.Y = size.Height - this.saturation * size.Height - 0.5f * size.Height;
        }

        private void OnSpectrumChange(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            this.hue = e.NewValue;
            this.gradientStop.Color = HsvToColor(this.hue, 1, 1);
            this.Update();
        }

        /// <summary>
        /// Based on https://stackoverflow.com/a/6930407/4778700
        /// </summary>
        private void SetHsvFromColor(Color color)
        {
            var r = color.R / (double)byte.MaxValue;
            var g = color.G / (double)byte.MaxValue;
            var b = color.B / (double)byte.MaxValue;
            double min, max, delta;

            min = r < g ? r : g;
            min = min < b ? min : b;

            max = r > g ? r : g;
            max = max > b ? max : b;

            double h, s;
            var v = max; // v
            delta = max - min;
            if (delta < 0.00001)
            {
                s = 0;
                h = 0; // undefined, maybe nan?
            }
            else
            {
                if (!(max > 0.0))
                {
                    // if max is 0, then r = g = b = 0              
                    // s = 0, h is undefined
                    s = 0.0;
                    h = double.NaN; // its now undefined
                }
                else
                {
                    // NOTE: if Max is == 0, this divide would cause a crash
                    s = (delta / max); // s

                    if (r >= max) // > is bogus, just keeps compilor happy
                    {
                        h = (g - b) / delta; // between yellow & magenta
                    }
                    else if (g >= max)
                    {
                        h = 2.0 + (b - r) / delta; // between cyan & yellow
                    }
                    else
                    {
                        h = 4.0 + (r - g) / delta; // between magenta & cyan
                    }

                    h *= 60.0; // degrees

                    if (h < 0.0)
                    {
                        h += 360.0;
                    }
                }
            }

            this.hue = h;
            this.saturation = s;
            this.value = v;
        }

        private void Update()
        {
            var c = HsvToColor(this.hue, this.saturation, this.value);
            int red = c.R;
            int green = c.G;
            int blue = c.B;
            this.textBlock.Text = $"#{red:X2}{green:X2}{blue:X2}";

            this.SetCurrentValue(SelectedColorProperty, c);

            this.ColorChanged?.Invoke(this, new ColorEventArgs(c));
        }

        private void UpdatePickerPosition(Point pos)
        {
            var size = this.hsGrid.RenderSize;

            pos.X = (float)Math.Max(0.0, Math.Min(size.Width,  pos.X));
            pos.Y = (float)Math.Max(0.0, Math.Min(size.Height, pos.Y));

            this.pickerTransform.X = pos.X - 0.5 * size.Width;
            this.pickerTransform.Y = pos.Y - 0.5 * size.Height;

            this.value = pos.X / size.Width;
            this.saturation = (size.Height - pos.Y) / size.Height;

            this.Update();
        }
    }
}