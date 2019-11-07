namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ControlMechEquipment : BaseUserControl
    {
        private readonly VehicleMechPrivateState mechPrivateState;

        private ViewModelControlMechEquipment viewModel;

        public ControlMechEquipment()
        {
        }

        public ControlMechEquipment(VehicleMechPrivateState mechPrivateState)
        {
            this.mechPrivateState = mechPrivateState;
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelControlMechEquipment(this.mechPrivateState);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }
    }
}