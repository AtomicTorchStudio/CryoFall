namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipArmorStats : BaseUserControl
    {
        private IProtoItemEquipment protoItem;

        private ViewModelItemTooltipArmorStats viewModel;

        public static ItemTooltipArmorStats Create(IProtoItemEquipment protoItem)
        {
            return new() { protoItem = protoItem };
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelItemTooltipArmorStats(this.protoItem);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}