namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemSlotFreshnessOverlayControl : BaseUserControl
    {
        private IItem item;

        private ViewModelItemFreshness viewModel;

        public static ItemSlotFreshnessOverlayControl Create(IItem item)
        {
            return new ItemSlotFreshnessOverlayControl() { item = item };
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelItemFreshness()
            {
                Item = this.item
            };
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }
    }
}