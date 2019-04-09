namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class IndicatorControl : BaseUserControl
    {
        private const string HiddenStateName = "Hidden";

        private const string NormalStateName = "Normal";

        public static readonly DependencyProperty DisplayGreenProperty =
            DependencyProperty.Register(nameof(DisplayGreen),
                                        typeof(ControlTemplate),
                                        typeof(IndicatorControl),
                                        new PropertyMetadata(default(ControlTemplate)));

        public static readonly DependencyProperty DisplayNoneProperty =
            DependencyProperty.Register(nameof(DisplayNone),
                                        typeof(ControlTemplate),
                                        typeof(IndicatorControl),
                                        new PropertyMetadata(default(ControlTemplate)));

        public static readonly DependencyProperty DisplayRedProperty =
            DependencyProperty.Register(nameof(DisplayRed),
                                        typeof(ControlTemplate),
                                        typeof(IndicatorControl),
                                        new PropertyMetadata(default(ControlTemplate)));

        public static readonly DependencyProperty DisplayYellowProperty =
            DependencyProperty.Register(nameof(DisplayYellow),
                                        typeof(ControlTemplate),
                                        typeof(IndicatorControl),
                                        new PropertyMetadata(default(ControlTemplate)));

        private string lastStateName;

        private ViewModelPerformanceStatsBase viewModel;

        public ControlTemplate DisplayGreen
        {
            get => (ControlTemplate)this.GetValue(DisplayGreenProperty);
            set => this.SetValue(DisplayGreenProperty, value);
        }

        public ControlTemplate DisplayNone
        {
            get => (ControlTemplate)this.GetValue(DisplayNoneProperty);
            set => this.SetValue(DisplayNoneProperty, value);
        }

        public ControlTemplate DisplayRed
        {
            get => (ControlTemplate)this.GetValue(DisplayRedProperty);
            set => this.SetValue(DisplayRedProperty, value);
        }

        public ControlTemplate DisplayYellow
        {
            get => (ControlTemplate)this.GetValue(DisplayYellowProperty);
            set => this.SetValue(DisplayYellowProperty, value);
        }

        protected override void InitControl()
        {
            this.GoToState(HiddenStateName, isInitial: true);
        }

        protected override void OnLoaded()
        {
            this.viewModel = (ViewModelPerformanceStatsBase)this.DataContext;
            this.viewModel.IndicatorSeverityChanged += this.IndicatorSeverityChangedHandler;
            this.MouseEnter += this.MouseEnterOrLeaveHandler;
            this.MouseLeave += this.MouseEnterOrLeaveHandler;
            this.Refresh(isInitial: true);
        }

        protected override void OnUnloaded()
        {
            this.viewModel.IndicatorSeverityChanged -= this.IndicatorSeverityChangedHandler;
            this.MouseEnter -= this.MouseEnterOrLeaveHandler;
            this.MouseLeave -= this.MouseEnterOrLeaveHandler;

            this.viewModel = null;
        }

        private void GoToState(string stateName, bool isInitial)
        {
            if (this.lastStateName == stateName)
            {
                return;
            }

            this.lastStateName = stateName;
            VisualStateManager.GoToState(this,
                                         stateName,
                                         useTransitions: !isInitial);
        }

        private void IndicatorSeverityChangedHandler()
        {
            this.Refresh(isInitial: false);
        }

        private void MouseEnterOrLeaveHandler(object sender, MouseEventArgs e)
        {
            this.Refresh(isInitial: false);
        }

        private void Refresh(bool isInitial)
        {
            var indicatorSeverity = this.viewModel.IndicatorSeverity;
            var isVisible = indicatorSeverity > PerformanceMetricSeverityLevel.Green
                            || this.IsMouseOver;

            var stateName = isVisible
                                ? NormalStateName
                                : HiddenStateName;

            this.GoToState(stateName, isInitial);
        }
    }
}