namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Quit
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuQuit : BaseUserControl
    {
        protected override void OnLoaded()
        {
            if (!IsDesignTime)
            {
                this.DataContext = new ViewModelMenuQuit();
            }
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            var viewModel = (BaseViewModel)this.DataContext;
            this.DataContext = null;
            viewModel.Dispose();
        }
    }
}