namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ObjectDoorPrivateState
        : StructurePrivateState,
          IObjectWithOwnersPrivateState,
          IObjectWithAccessModePrivateState
    {
        [SyncToClient]
        public WorldObjectAccessMode AccessMode { get; set; }

        [SyncToClient]
        public NetworkSyncList<string> Owners { get; set; }
    }
}