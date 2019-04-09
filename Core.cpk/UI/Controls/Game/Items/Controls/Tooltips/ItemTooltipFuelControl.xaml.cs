namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipFuelControl : BaseUserControl
    {
        private IItem item;

        private ViewModelItemFuel viewModel;

        public static ItemTooltipFuelControl Create(IItem item)
        {
            return new ItemTooltipFuelControl() { item = item };
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelItemFuel()
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