namespace AtomicTorch.CBND.CoreMod
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public interface IPublicStateWithStructurePoints : IPublicState
    {
        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: ScriptingConstants.NetworkDefaultMaxUpdatesPerSecond)]
        float StructurePointsCurrent { get; set; }
    }
}