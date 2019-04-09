namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelWindowConstructionMenuHotbarHelper : IDisposable
    {
        private readonly IClientItemsContainer hotbarContainer;

        private readonly Action<IClientItemsContainer> onHotbarUpdated;

        public ViewModelWindowConstructionMenuHotbarHelper(Action<IClientItemsContainer> onHotbarUpdated)
        {
            this.onHotbarUpdated = onHotbarUpdated;
            this.hotbarContainer = ClientHotbarSelectedItemManager.ContainerHotbar;

            this.hotbarContainer.StateHashChanged += this.HotbarContainerStateHashChangedHandler;
        }

        public IClientItemsContainer HotbarContainer => this.hotbarContainer;

        public void Dispose()
        {
            this.hotbarContainer.StateHashChanged -= this.HotbarContainerStateHashChangedHandler;
        }

        private void HotbarContainerStateHashChangedHandler()
        {
            this.onHotbarUpdated(this.hotbarContainer);
        }
    }
}