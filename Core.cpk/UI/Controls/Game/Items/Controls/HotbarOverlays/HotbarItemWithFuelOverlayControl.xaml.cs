namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HotbarItemWithFuelOverlayControl : BaseUserControl
    {
        private readonly IItem item;

        private readonly IReadOnlyItemFuelConfig itemFuelConfig;

        private ViewModelHotbarItemWithFuelOverlayControl viewModel;

        public HotbarItemWithFuelOverlayControl()
        {
        }

        public HotbarItemWithFuelOverlayControl(IItem item, IReadOnlyItemFuelConfig itemFuelConfig)
        {
            this.item = item;
            this.itemFuelConfig = itemFuelConfig;
        }

        protected override void InitControl()
        {
            this.DataContext = this.viewModel =
                                   new ViewModelHotbarItemWithFuelOverlayControl(this.item, this.itemFuelConfig);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}