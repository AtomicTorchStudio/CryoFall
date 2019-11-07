namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemsContainerExchangeControl : BaseUserControl
    {
        protected override void OnLoaded()
        {
            if (this.DataContext is ViewModelItemsContainerExchange viewModel)
            {
                viewModel.IsActive = true;
            }
        }

        protected override void OnUnloaded()
        {
            if (this.DataContext is ViewModelItemsContainerExchange viewModel)
            {
                viewModel.IsActive = false;
            }
        }
    }
}