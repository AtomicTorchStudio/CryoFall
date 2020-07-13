namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelVehicleEnergy : BaseViewModel
    {
        private readonly IDynamicWorldObject vehicle;

        private readonly VehiclePrivateState vehiclePrivateState;

        public ViewModelVehicleEnergy(IDynamicWorldObject vehicle)
        {
            this.vehicle = vehicle;
            this.vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();
            this.vehiclePrivateState.ClientSubscribe(_ => _.CurrentEnergyMax,
                                                     _ => this.NotifyPropertyChanged(nameof(this.EnergyMax)),
                                                     this);

            this.RefreshEnergy();
        }

        public double EnergyCurrent { get; private set; }

        public double EnergyMax => this.vehiclePrivateState.CurrentEnergyMax;

        private void RefreshEnergy()
        {
            if (this.IsDisposed
                || !this.vehicle.ClientHasPrivateState)
            {
                return;
            }

            this.EnergyCurrent = VehicleEnergySystem.SharedCalculateTotalEnergyCharge(this.vehicle);
            ClientTimersSystem.AddAction(delaySeconds: 0.25, this.RefreshEnergy);
        }
    }
}