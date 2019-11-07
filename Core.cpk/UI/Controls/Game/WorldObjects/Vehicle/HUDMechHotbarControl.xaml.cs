namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDMechHotbarControl : BaseUserControl
    {
        private readonly IDynamicWorldObject vehicle;

        private ViewModelHUDMechHotbarControl viewModel;

        public HUDMechHotbarControl()
        {
        }

        public HUDMechHotbarControl(IDynamicWorldObject vehicle)
        {
            this.vehicle = vehicle;
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelHUDMechHotbarControl(this.vehicle);
            var hotbarItemSlotControl0 = this.GetByName<HotbarItemSlotControl>("HotbarItemSlotControl0");
            hotbarItemSlotControl0.Setup(this.viewModel.EquipmentItemsContainer, slotId: 0);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }
    }
}