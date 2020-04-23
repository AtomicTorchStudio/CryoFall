namespace AtomicTorch.CBND.CoreMod.Events.Base
{
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoEvent : IProtoGameObject
    {
        string Description { get; }

        ITextureResource Icon { get; }

        void ServerForceCreateAndStart();

        bool ServerIsTriggerAllowed(ProtoTrigger trigger);

        string SharedGetProgressText(ILogicObject activeEvent);
    }
}