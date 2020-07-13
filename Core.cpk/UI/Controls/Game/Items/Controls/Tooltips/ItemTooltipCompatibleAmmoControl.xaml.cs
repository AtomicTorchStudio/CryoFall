namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipCompatibleAmmoControl : BaseUserControl
    {
        private IProtoItemWeapon protoItemWeapon;

        private ViewModelItemTooltipCompatibleAmmoControl viewModel;

        public static ItemTooltipCompatibleAmmoControl Create(IProtoItemWeapon protoItemWeapon)
        {
            return new ItemTooltipCompatibleAmmoControl() { protoItemWeapon = protoItemWeapon };
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelItemTooltipCompatibleAmmoControl(this.protoItemWeapon);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}