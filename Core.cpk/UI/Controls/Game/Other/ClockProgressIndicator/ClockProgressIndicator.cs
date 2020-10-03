namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.ClockProgressIndicator
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClockProgressIndicator : BaseContentControl
    {
        public static readonly DependencyProperty ProgressFractionProperty =
            DependencyProperty.Register(nameof(ProgressFraction),
                                        typeof(double),
                                        typeof(ClockProgressIndicator),
                                        new PropertyMetadata(default(double), ProgressPropertyChanged));

        private Grid layoutRoot;

        private ViewModelClockProgressIndicator viewModel;

        static ClockProgressIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ClockProgressIndicator),
                new FrameworkPropertyMetadata(typeof(ClockProgressIndicator)));
        }

        public double ProgressFraction
        {
            get => (double)this.GetValue(ProgressFractionProperty);
            set => this.SetValue(ProgressFractionProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = (Grid)VisualTreeHelper.GetChild(this, 0);
        }

        protected override void OnLoaded()
        {
            if (this.Content is null)
            {
                // setup default content
                var ellipse = new Ellipse();
                BindingOperations.SetBinding(
                    ellipse,
                    Shape.FillProperty,
                    new Binding(ForegroundProperty.Name)
                    {
                        Source = this,
                        Mode = BindingMode.OneWay
                    });
                this.Content = ellipse;
            }

            this.viewModel = new ViewModelClockProgressIndicator(isReversed: false);
            this.layoutRoot.DataContext = this.viewModel;

            this.UpdateSize();
            this.SizeChanged += this.SizeChangedHandler;

            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
            this.SizeChanged -= this.SizeChangedHandler;
        }

        private static void ProgressPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ClockProgressIndicator)d).Refresh();
        }

        private void Refresh()
        {
            if (this.viewModel is not null)
            {
                this.viewModel.ProgressFraction = this.ProgressFraction;
            }
        }

        private void SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            this.UpdateSize();
        }

        private void UpdateSize()
        {
            this.viewModel.ControlSize = new Vector2Ushort((ushort)this.ActualWidth, (ushort)this.ActualHeight);
        }
    }
}