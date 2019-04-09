namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Windows.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelWindowContainerExchange : BaseViewModel
    {
        public ViewModelWindowContainerExchange(IItemsContainer container, Action callbackCloseWindow)
        {
            this.ViewModel = new ViewModelItemsContainerExchange(
                container,
                callbackTakeAllItemsSuccess: callbackCloseWindow);
        }

        public ViewModelItemsContainerExchange ViewModel { get; }
    }
}