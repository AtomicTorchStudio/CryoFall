namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionsListControl : BaseUserControl
    {
        public static readonly DependencyProperty EmptyListMessageProperty =
            DependencyProperty.Register("EmptyListMessage",
                                        typeof(string),
                                        typeof(FactionsListControl),
                                        new PropertyMetadata(default(string)));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive",
                                        typeof(bool),
                                        typeof(FactionsListControl),
                                        new PropertyMetadata(false, IsActivePropertyChanged));

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode),
                                        typeof(FactionListControlMode),
                                        typeof(FactionsListControl),
                                        new PropertyMetadata(default(FactionListControlMode)));

        private ViewModelFactionsListControl viewModel;

        public string EmptyListMessage
        {
            get => (string)this.GetValue(EmptyListMessageProperty);
            set => this.SetValue(EmptyListMessageProperty, value);
        }

        public bool IsActive
        {
            get => (bool)this.GetValue(IsActiveProperty);
            set => this.SetValue(IsActiveProperty, value);
        }

        public FactionListControlMode Mode
        {
            get => (FactionListControlMode)this.GetValue(ModeProperty);
            set => this.SetValue(ModeProperty, value);
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelFactionsListControl(this.Mode);
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private static void IsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FactionsListControl)d;
            control.Refresh();
        }

        private void Refresh()
        {
            if (this.viewModel is null)
            {
                return;
            }

            this.viewModel.IsActive = this.isLoaded
                                      && this.IsActive;
        }
    }
}