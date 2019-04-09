namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IInteractableProtoStaticWorldObject : IProtoStaticWorldObject
    {
        bool IsAutoEnterPrivateScopeOnInteraction { get; }

        BaseUserControlWithWindow ClientOpenUI(IStaticWorldObject worldObject);

        void ServerOnClientInteract(ICharacter who, IStaticWorldObject worldObject);

        void ServerOnMenuClosed(ICharacter who, IStaticWorldObject worldObject);
    }
}