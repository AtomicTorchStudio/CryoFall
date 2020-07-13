namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipCompatibleWeaponsControl : BaseUserControl
    {
        private IProtoItemAmmo protoItemAmmo;

        private ViewModelItemTooltipCompatibleWeaponsControl viewModel;

        public static ItemTooltipCompatibleWeaponsControl Create(IProtoItemAmmo protoItemAmmo)
        {
            return new ItemTooltipCompatibleWeaponsControl() { protoItemAmmo = protoItemAmmo };
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelItemTooltipCompatibleWeaponsControl(this.protoItemAmmo);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}