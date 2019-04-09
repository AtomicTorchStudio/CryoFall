namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.CurrentGame
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.CurrentGame.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuCurrentGame : BaseUserControl
    {
        private ViewModelMenuCurrentGame viewModel;

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = this.viewModel = new ViewModelMenuCurrentGame();
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}