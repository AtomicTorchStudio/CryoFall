namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ObjectCratePrivateState
        : StructurePrivateState,
          IObjectWithOwnersPrivateState,
          IObjectWithAccessModePrivateState
    {
        [SyncToClient]
        public WorldObjectDirectAccessMode DirectAccessMode { get; set; }

        [SyncToClient]
        public WorldObjectFactionAccessModes FactionAccessMode { get; set; }

        [SyncToClient]
        public IItemsContainer ItemsContainer { get; set; }

        [SyncToClient]
        public NetworkSyncList<string> Owners { get; set; }
    }
}