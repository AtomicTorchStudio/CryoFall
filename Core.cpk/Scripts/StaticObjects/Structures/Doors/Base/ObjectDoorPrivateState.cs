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

        /// <summary>
        /// (PvP) Is the door blocked by shield protection?
        /// Please note this value is not persistent as the shield protection system
        /// automatically reapplies this state on the savegame loading.
        /// </summary>
        [SyncToClient]
        [TempOnly]
        public bool IsBlockedByShield { get; set; }

        [SyncToClient]
        public NetworkSyncList<string> Owners { get; set; }
    }
}