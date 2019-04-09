namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectBushPublicState : VegetationPublicState
    {
        [SyncToClient(DeliveryMode.ReliableSequenced)]
        public bool HasHarvest { get; set; }
    }
}