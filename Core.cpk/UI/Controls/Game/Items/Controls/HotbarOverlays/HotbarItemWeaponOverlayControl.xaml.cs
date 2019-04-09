namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HotbarItemWeaponOverlayControl : BaseUserControl
    {
        private readonly IItem item;

        private ViewModelHotbarItemWeaponOverlayControl viewModel;

        public HotbarItemWeaponOverlayControl()
        {
        }

        public HotbarItemWeaponOverlayControl(IItem item)
        {
            this.item = item;
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelHotbarItemWeaponOverlayControl()
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