namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelVehicleEnergy : BaseViewModel
    {
        private readonly IProtoVehicle protoVehicle;

        private readonly IDynamicWorldObject vehicle;

        private readonly VehiclePrivateState vehiclePrivateState;

        public ViewModelVehicleEnergy(IDynamicWorldObject vehicle)
        {
            this.vehicle = vehicle;
            this.protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
            this.vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();

            this.RefreshEnergy();
        }

        public double EnergyCurrent { get; private set; }

        public double EnergyMax => this.protoVehicle.EnergyMax;

        private void RefreshEnergy()
        {
            if (this.IsDisposed)
            {
                return;
            }

            ClientTimersSystem.AddAction(delaySeconds: 0.25, this.RefreshEnergy);

            var energyCurrent = 0u;
            foreach (var item in this.vehiclePrivateState.FuelItemsContainer.Items)
            {
                if (item.ProtoItem is IProtoItemWithDurability)
                {
                    energyCurrent += item.GetPrivateState<ItemWithDurabilityPrivateState>().DurabilityCurrent;
                }
            }

            this.EnergyCurrent = energyCurrent;
        }
    }
}