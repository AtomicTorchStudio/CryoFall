namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectGeneratorBioPrivateState : ObjectGeneratorPrivateState
    {
        [SyncToClient]
        public IItemsContainer InputItemsCointainer { get; set; }

        [SyncToClient(DeliveryMode.UnreliableSequenced, maxUpdatesPerSecond: 1)]
        public float OrganicAmount { get; set; }
    }
}