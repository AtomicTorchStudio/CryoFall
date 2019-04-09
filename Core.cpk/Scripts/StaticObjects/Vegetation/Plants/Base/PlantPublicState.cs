namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class PlantPublicState : VegetationPublicState
    {
        [SyncToClient]
        public bool HasHarvest { get; set; }

        [SyncToClient]
        public bool IsWatered { get; set; }

        public void ServerOnFertilizerApplied()
        {
            // this is a hack to force notification on Client-side
            this.ForceSyncPropertySend(this.HasHarvest, nameof(this.HasHarvest));
        }

        internal void ServerForceIsWateredSync()
        {
            this.ForceSyncPropertySend(this.IsWatered, nameof(this.IsWatered));
        }
    }
}