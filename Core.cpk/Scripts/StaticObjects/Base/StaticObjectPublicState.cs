namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class StaticObjectPublicState
        : BasePublicState,
          IPublicStateWithStructurePoints,
          IWorldObjectPublicStateWithClaim
    {
        [SyncToClient]
        [TempOnly]
        public ILogicObject WorldObjectClaim { get; set; }

        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: ScriptingConstants.NetworkDefaultMaxUpdatesPerSecond)]
        public float StructurePointsCurrent { get; set; }
    }
}