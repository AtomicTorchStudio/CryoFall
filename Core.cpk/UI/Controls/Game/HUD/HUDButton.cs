namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class HUDButton : BaseControl
    {
        public static readonly DependencyProperty BrushHighlightedProperty =
            DependencyProperty.Register(
                nameof(BrushHighlighted),
                typeof(Brush),
                typeof(HUDButton),
                new PropertyMetadata(Brushes.Red));

        public static readonly DependencyProperty BrushNormalProperty =
            DependencyProperty.Register(
                nameof(BrushNormal),
                typeof(Brush),
                typeof(HUDButton),
                new PropertyMetadata(Brushes.Green));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(BaseCommand),
                typeof(HUDButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(HUDButton),
                new PropertyMetadata(default(bool), IsSelectedPropertyChanged));

        public static readonly DependencyProperty BadgeContentProperty =
            DependencyProperty.Register(nameof(BadgeContent),
                                        typeof(FrameworkElement),
                                        typeof(HUDButton),
                                        new PropertyMetadata(default(Control)));

        static HUDButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HUDButton),
                new FrameworkPropertyMetadata(typeof(HUDButton)));
        }

        public FrameworkElement BadgeContent
        {
            get => (FrameworkElement)this.GetValue(BadgeContentProperty);
            set => this.SetValue(BadgeContentProperty, value);
        }

        public Brush BrushHighlighted
        {
            get => (Brush)this.GetValue(BrushHighlightedProperty);
            set => this.SetValue(BrushHighlightedProperty, value);
        }

        public Brush BrushNormal
        {
            get => (Brush)this.GetValue(BrushNormalProperty);
            set => this.SetValue(BrushNormalProperty, value);
        }

        public BaseCommand Command
        {
            get => (BaseCommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        protected override void OnLoaded()
        {
            this.MouseEnter += this.HUDButtonMouseEnterOrLeaveHandler;
            this.MouseLeave += this.HUDButtonMouseEnterOrLeaveHandler;

            this.UpdateVisualState(useTransitions: false);

            var badgeContent = this.BadgeContent;
            if (badgeContent != null)
            {
                // workaround for NoesisGUI 3.0
                var child = (FrameworkElement)VisualTreeHelper.GetChild(badgeContent, 0);
                child.DataContext = this.DataContext;
            }
        }

        protected override void OnUnloaded()
        {
            this.MouseEnter -= this.HUDButtonMouseEnterOrLeaveHandler;
            this.MouseLeave -= this.HUDButtonMouseEnterOrLeaveHandler;
        }

        private static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HUDButton)d).OnIsSelectedPropertyChanged();
        }

        private void HUDButtonMouseEnterOrLeaveHandler(object sender, MouseEventArgs e)
        {
            this.UpdateVisualState();
        }

        private void OnIsSelectedPropertyChanged()
        {
            this.UpdateVisualState();
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private void UpdateVisualState(bool useTransitions = true)
        {
            VisualStateManager.GoToState(
                this,
                this.IsMouseOver || this.IsSelected
                    ? "Expanded"
                    : "Collapsed",
                useTransitions);
        }
    }
}