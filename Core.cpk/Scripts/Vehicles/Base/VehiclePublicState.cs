namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class VehiclePublicState : BasePublicState, IPublicStateWithStructurePoints
    {
        [SyncToClient(deliveryMode: DeliveryMode.ReliableSequenced)]
        public bool IsLightsEnabled { get; set; }

        [TempOnly]
        [SubscribableProperty]
        public bool IsDismountRequested { get; set; }

        [SyncToClient]
        public ICharacter PilotCharacter { get; set; }

        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: ScriptingConstants.NetworkDefaultMaxUpdatesPerSecond)]
        public float StructurePointsCurrent { get; set; }
    }
}