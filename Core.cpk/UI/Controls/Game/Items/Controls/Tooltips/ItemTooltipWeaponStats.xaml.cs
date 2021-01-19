namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipWeaponStats : BaseUserControl
    {
        private IItem item;

        private IProtoItemWeapon protoItem;

        private ViewModelItemTooltipWeaponStats viewModel;

        public static ItemTooltipWeaponStats Create(
            IItem item,
            IProtoItemWeapon protoItem)
        {
            return new() { item = item, protoItem = protoItem };
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelItemTooltipWeaponStats(this.item,
                                   this.protoItem);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}