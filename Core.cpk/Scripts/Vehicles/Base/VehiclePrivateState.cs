namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class VehiclePrivateState
        : BasePrivateState, IObjectWithOwnersPrivateState
    {
        [SyncToClient]
        public IItemsContainer CargoItemsContainer { get; set; }

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