namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Account
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Account.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ExternalAccountLinkControl : BaseUserControl
    {
        public static readonly DependencyProperty CommandBackProperty
            = DependencyProperty.Register(nameof(CommandBack),
                                          typeof(BaseCommand),
                                          typeof(ExternalAccountLinkControl),
                                          new PropertyMetadata(default(BaseCommand)));

        private FrameworkElement root;

        private ViewModelLinkExternalAccountFormControl viewModel;

        public BaseCommand CommandBack
        {
            get => (BaseCommand)this.GetValue(CommandBackProperty);
            set => this.SetValue(CommandBackProperty, value);
        }

        protected override void OnLoaded()
        {
            this.root = this.GetByName<Grid>("LayoutRoot");
            this.root.DataContext = this.viewModel = new ViewModelLinkExternalAccountFormControl();
        }

        protected override void OnUnloaded()
        {
            this.root.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}