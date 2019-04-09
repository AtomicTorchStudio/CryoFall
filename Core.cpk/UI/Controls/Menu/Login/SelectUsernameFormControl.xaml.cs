namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class SelectUsernameFormControl : BaseUserControl
    {
        private ViewModelSelectUsernameFormControl viewModel;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelSelectUsernameFormControl();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}