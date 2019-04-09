namespace AtomicTorch.CBND.CoreMod.Quests
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class QuestRequirementState : BaseNetObject
    {
        [SyncToClient(DeliveryMode.ReliableSequenced, maxUpdatesPerSecond: 1)]
        public bool IsSatisfied { get; set; }
    }
}