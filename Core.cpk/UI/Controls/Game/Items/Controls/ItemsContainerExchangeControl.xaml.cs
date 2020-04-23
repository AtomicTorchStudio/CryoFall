namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemsContainerExchangeControl : BaseUserControl
    {
        public const bool IsEnabledOnlyWhenLoaded = false;

        protected override void OnLoaded()
        {
            if (IsEnabledOnlyWhenLoaded
                && this.DataContext is ViewModelItemsContainerExchange viewModel)
            {
                viewModel.IsActive = true;
            }
        }

        protected override void OnUnloaded()
        {
            if (IsEnabledOnlyWhenLoaded
                && this.DataContext is ViewModelItemsContainerExchange viewModel)
            {
                viewModel.IsActive = false;
            }
        }
    }
}