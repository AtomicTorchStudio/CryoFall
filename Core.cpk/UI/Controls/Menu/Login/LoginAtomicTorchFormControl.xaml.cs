namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class LoginAtomicTorchFormControl : BaseUserControl
    {
        private ViewModelLoginAtomicTorchFormControl viewModel;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelLoginAtomicTorchFormControl();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}