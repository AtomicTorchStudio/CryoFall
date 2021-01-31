namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class VehiclePrivateState
        : BasePrivateState,
          IObjectWithOwnersPrivateState,
          IObjectWithAccessModePrivateState
    {
        [SyncToClient]
        public IItemsContainer CargoItemsContainer { get; set; }

        [SyncToClient]
        public double CurrentEnergyMax { get; set; }

        /// <summary>
        /// The direct access mode is never used for vehicles.
        /// Only faction access mode is used (for faction-owned vehicles).
        /// </summary>
        public WorldObjectDirectAccessMode DirectAccessMode
        {
            get => WorldObjectDirectAccessMode.OpensToObjectOwners;
            set { }
        }

        [SyncToClient]
        public WorldObjectFactionAccessModes FactionAccessMode { get; set; }
            = WorldObjectFactionAccessModes.AllFactionMembers;

        [SyncToClient]
        public IItemsContainer FuelItemsContainer { get; set; }

        [SyncToClient]
        public bool IsInGarage { get; set; }

        [SyncToClient]
        public NetworkSyncList<string> Owners { get; set; }

        public ICharacter ServerLastPilotCharacter { get; set; }

        public double ServerTimeSinceLastUse { get; set; }

        [TempOnly]
        public double ServerTimeSincePilotOffline { get; set; }
    }
}