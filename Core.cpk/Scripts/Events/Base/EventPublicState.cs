namespace AtomicTorch.CBND.CoreMod.Events
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class EventPublicState : BasePublicState
    {
        [SyncToClient]
        public double EventEndTime { get; set; }
    }
}