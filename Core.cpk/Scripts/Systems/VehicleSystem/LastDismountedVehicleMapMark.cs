namespace AtomicTorch.CBND.CoreMod.Systems.VehicleSystem
{
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public readonly struct LastDismountedVehicleMapMark : IRemoteCallParameter
    {
        public LastDismountedVehicleMapMark(IDynamicWorldObject vehicle)
        {
            this.ProtoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
            this.Position = vehicle.Position;
        }

        [SyncToClient]
        public IProtoVehicle ProtoVehicle { get; }

        [SyncToClient]
        public Vector2D Position { get; }
    }
}