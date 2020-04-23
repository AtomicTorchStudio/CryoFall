namespace AtomicTorch.CBND.CoreMod.Events.Base
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class EventPublicState : BasePublicState
    {
        [SyncToClient]
        public double EventEndTime { get; set; }
    }
}