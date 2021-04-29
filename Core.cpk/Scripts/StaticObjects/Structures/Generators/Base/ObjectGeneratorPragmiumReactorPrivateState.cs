namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ObjectGeneratorPragmiumReactorPrivateState : BaseNetObject
    {
        [SyncToClient(DeliveryMode.ReliableSequenced, networkDataType: typeof(float))]
        public double ActivationProgressPercents { get; set; }

        [SyncToClient]
        public bool IsEnabled { get; set; }

        [SyncToClient]
        public IItemsContainer ItemsContainer { get; set; }

        [TempOnly]
        public double ServerAccumulatedDecayDuration { get; set; }

        [TempOnly]
        public ushort? ServerItemsContainerLastStateHash { get; set; }

        [SyncToClient]
        [TempOnly]
        public PragmiumReactorStatsData Stats { get; set; }
    }
}