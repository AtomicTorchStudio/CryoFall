namespace AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem
{
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public readonly struct GarageVehicleEntry : IRemoteCallParameter
    {
        public readonly uint Id;

        public readonly IProtoVehicle ProtoVehicle;

        public readonly VehicleStatus Status;

        public GarageVehicleEntry(IWorldObject vehicle, VehicleStatus status)
        {
            this.Id = vehicle.Id;
            this.ProtoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
            this.Status = status;
        }
    }
}