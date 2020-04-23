namespace AtomicTorch.CBND.CoreMod.Events.Base
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class EventDropPublicState : EventWithAreaPublicState
    {
        [SyncToClient]
        public byte ObjectsRemains { get; set; }

        [SyncToClient]
        public byte ObjectsTotal { get; set; }
    }
}