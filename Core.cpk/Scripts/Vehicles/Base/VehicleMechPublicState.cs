namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class VehicleMechPublicState : VehiclePublicState
    {
        [SyncToClient]
        [TempOnly]
        public IProtoItem ProtoItemLeftTurretSlot { get; set; }

        [SyncToClient]
        [TempOnly]
        public IProtoItem ProtoItemRightTurretSlot { get; set; }
    }
}