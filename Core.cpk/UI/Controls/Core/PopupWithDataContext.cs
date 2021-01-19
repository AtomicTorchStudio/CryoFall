namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class PopupWithDataContext : BaseContentControl
    {
        public static readonly DependencyProperty IsOpenProperty
            = DependencyProperty.Register("IsOpen",
                                          typeof(bool),
                                          typeof(PopupWithDataContext),
                                          new PropertyMetadata(default(bool), IsOpenPropertyChanged));

        public static readonly DependencyProperty AutoCloseOnContentClickProperty =
            DependencyProperty.Register("AutoCloseOnContentClick",
                                        typeof(bool),
                                        typeof(PopupWithDataContext),
                                        new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CommandCloseProperty =
            DependencyProperty.Register("CommandClose",
                                        typeof(BaseCommand),
                                        typeof(PopupWithDataContext),
                                        new PropertyMetadata(default(BaseCommand)));

        public static readonly DependencyProperty CommandOpenProperty =
            DependencyProperty.Register("CommandOpen",
                                        typeof(BaseCommand),
                                        typeof(PopupWithDataContext),
                                        new PropertyMetadata(default(BaseCommand)));

        public static readonly DependencyProperty CommandToggleProperty =
            DependencyProperty.Register("CommandToggle",
                                        typeof(BaseCommand),
                                        typeof(PopupWithDataContext),
                                        new PropertyMetadata(default(BaseCommand)));

        private Popup popup;

        static PopupWithDataContext()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PopupWithDataContext),
                new FrameworkPropertyMetadata(typeof(PopupWithDataContext)));
        }

        public bool AutoCloseOnContentClick
        {
            get => (bool)this.GetValue(AutoCloseOnContentClickProperty);
            set => this.SetValue(AutoCloseOnContentClickProperty, value);
        }

        public BaseCommand CommandClose
        {
            get => (BaseCommand)this.GetValue(CommandCloseProperty);
            set => this.SetValue(CommandCloseProperty, value);
        }

        public BaseCommand CommandOpen
        {
            get => (BaseCommand)this.GetValue(CommandOpenProperty);
            set => this.SetValue(CommandOpenProperty, value);
        }

        public BaseCommand CommandToggle
        {
            get => (BaseCommand)this.GetValue(CommandToggleProperty);
            set => this.SetValue(CommandToggleProperty, value);
        }

        public bool IsOpen
        {
            get => (bool)this.GetValue(IsOpenProperty);
            set => this.SetValue(IsOpenProperty, value);
        }

        protected override void InitControl()
        {
            this.CommandOpen = new ActionCommand(this.ExecuteCommandOpen);
            this.CommandClose = new ActionCommand(this.ExecuteCommandClose);
            this.CommandToggle = new ActionCommand(this.ExecuteCommandToggle);
        }

        protected override void OnLoaded()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.popup = templateRoot.GetByName<Popup>("Popup");
            this.popup.PreviewMouseDown += this.PopupOnMouseDown;
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.Refresh();
            this.popup.PreviewMouseDown -= this.PopupOnMouseDown;
            this.popup = null;
        }

        private static void IsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupWithDataContext)d).Refresh();
        }

        private void ExecuteCommandClose()
        {
            this.IsOpen = false;
        }

        private void ExecuteCommandOpen()
        {
            this.IsOpen = true;
        }

        private void ExecuteCommandToggle()
        {
            this.IsOpen = !this.IsOpen;
        }

        private void PopupOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.AutoCloseOnContentClick)
            {
                ClientTimersSystem.AddAction(0,
                                             () => this.IsOpen = false);
            }
        }

        private void Refresh()
        {
            if (this.popup is null)
            {
                return;
            }

            var isOpen = this.isLoaded
                         && this.IsOpen;

            this.popup.IsOpen = isOpen;
            this.popup.DataContext = isOpen ? this.DataContext : null;
        }
    }
}