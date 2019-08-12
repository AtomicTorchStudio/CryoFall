namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class PlantPublicState : VegetationPublicState
    {
        [SyncToClient]
        public bool HasHarvest { get; set; }

        [TempOnly]
        [SyncToClient]
        public bool IsFertilized { get; set; }

        [SyncToClient]
        public bool IsWatered { get; set; }

        internal void ServerForceIsFertilizedSync()
        {
            this.ForceSyncPropertySend(this.IsFertilized, nameof(this.IsFertilized));
        }

        internal void ServerForceIsWateredSync()
        {
            this.ForceSyncPropertySend(this.IsWatered, nameof(this.IsWatered));
        }
    }
}