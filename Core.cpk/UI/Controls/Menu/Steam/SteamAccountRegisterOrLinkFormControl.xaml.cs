namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class SteamAccountRegisterOrLinkFormControl : BaseUserControl
    {
        public static readonly DependencyProperty CommandBackProperty =
            DependencyProperty.Register(nameof(CommandBack),
                                        typeof(BaseCommand),
                                        typeof(SteamAccountRegisterOrLinkFormControl),
                                        new PropertyMetadata(default(BaseCommand)));

        private FrameworkElement root;

        private ViewModelSteamAccountRegisterOrLinkFormControl viewModel;

        public BaseCommand CommandBack
        {
            get => (BaseCommand)this.GetValue(CommandBackProperty);
            set => this.SetValue(CommandBackProperty, value);
        }

        protected override void OnLoaded()
        {
            this.root = this.GetByName<Grid>("LayoutRoot");
            this.root.DataContext = this.viewModel = new ViewModelSteamAccountRegisterOrLinkFormControl();
        }

        protected override void OnUnloaded()
        {
            this.root.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}