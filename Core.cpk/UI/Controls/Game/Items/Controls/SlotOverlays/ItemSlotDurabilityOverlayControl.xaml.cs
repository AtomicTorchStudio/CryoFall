namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemSlotDurabilityOverlayControl : BaseUserControl
    {
        private IItem item;

        private ViewModelItemDurability viewModel;

        public static ItemSlotDurabilityOverlayControl Create(IItem item)
        {
            return new ItemSlotDurabilityOverlayControl() { item = item };
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelItemDurability()
            {
                Item = this.item
            };
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}