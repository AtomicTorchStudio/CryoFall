namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipSolarPanelControl : BaseUserControl
    {
        private IItem item;

        private IProtoItemSolarPanel protoItemSolarPanel;

        private ViewModelItemTooltipSolarPanelControl viewModel;

        public static ItemTooltipSolarPanelControl Create(IItem item)
        {
            return new() { item = item };
        }

        public static ItemTooltipSolarPanelControl Create(IProtoItemSolarPanel protoItemSolarPanel)
        {
            return new() { protoItemSolarPanel = protoItemSolarPanel };
        }

        protected override void OnLoaded()
        {
            this.DataContext
                = this.viewModel
                      = this.item is not null
                            ? new ViewModelItemTooltipSolarPanelControl(this.item)
                            : new ViewModelItemTooltipSolarPanelControl(this.protoItemSolarPanel);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}