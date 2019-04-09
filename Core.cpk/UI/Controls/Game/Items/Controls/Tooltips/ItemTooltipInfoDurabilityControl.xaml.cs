namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipInfoDurabilityControl : BaseUserControl
    {
        private IItem item;

        private ViewModelItemDurability viewModel;

        public static ItemTooltipInfoDurabilityControl Create(IItem item)
        {
            return new ItemTooltipInfoDurabilityControl() { item = item };
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