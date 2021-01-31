namespace AtomicTorch.CBND.CoreMod.Events
{
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoEvent : IProtoGameObject
    {
        bool ConsolidateNotifications { get; }

        string Description { get; }

        ITextureResource Icon { get; }

        /// <summary>
        /// Determines when this event was active last time.
        /// </summary>
        double ServerLastActiveTime { get; }

        void ServerForceCreateAndStart();

        bool ServerIsTriggerAllowed(ProtoTrigger trigger);

        string SharedGetProgressText(ILogicObject activeEvent);
    }
}