namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class RechargingStationItemSlot : BaseUserControl, IItemSlotControl
    {
        private ItemSlotControl itemSlotControl;

        private ViewModelRechargingStationItemSlot viewModel;

        public void RefreshItem()
        {
            if (!this.isLoaded)
            {
                return;
            }

            this.itemSlotControl.RefreshItem();

            this.viewModel.Item = this.itemSlotControl.Container?
                .GetItemAtSlot(this.itemSlotControl.SlotId);
        }

        public void ResetControlForCache()
        {
            this.itemSlotControl.ResetControlForCache();
        }

        public void Setup(IItemsContainer setContainer, byte slotId)
        {
            this.itemSlotControl.Setup(setContainer, slotId);
            this.RefreshItem();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            this.Cleanup();
        }

        protected override void InitControl()
        {
            this.itemSlotControl = this.GetByName<ItemSlotControl>("ItemSlotControl");
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelRechargingStationItemSlot();
            this.RefreshItem();
        }

        protected override void OnUnloaded()
        {
            this.Cleanup();
        }

        private void Cleanup()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}