namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class StaticObjectPublicState : BasePublicState
    {
        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: ScriptingConstants.NetworkDefaultMaxUpdatesPerSecond)]
        public float StructurePointsCurrent { get; set; }
    }
}