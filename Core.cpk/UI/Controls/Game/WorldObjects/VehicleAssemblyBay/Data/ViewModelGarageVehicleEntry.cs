namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.VehicleAssemblyBay
{
    using AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelGarageVehicleEntry : BaseViewModel
    {
        private VehicleStatus status;

        public ViewModelGarageVehicleEntry(
            uint vehicleGameObjectId,
            IProtoVehicle protoVehicle,
            VehicleStatus status)
        {
            this.VehicleGameObjectId = vehicleGameObjectId;
            this.ProtoVehicle = protoVehicle;
            this.Status = status;
        }

        public TextureBrush Icon => Api.Client.UI.GetTextureBrush(this.ProtoVehicle.Icon);

        public IProtoVehicle ProtoVehicle { get; }

        public VehicleStatus Status
        {
            get => this.status;
            set
            {
                if (this.status == value)
                {
                    return;
                }

                this.status = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.StatusText));
            }
        }

        public string StatusText => this.Status.GetDescription();

        public string Title => this.ProtoVehicle.Name;

        public uint VehicleGameObjectId { get; }
    }
}