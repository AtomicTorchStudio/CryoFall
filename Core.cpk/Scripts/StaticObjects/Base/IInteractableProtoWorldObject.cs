namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IInteractableProtoWorldObject : IProtoWorldObject
    {
        bool IsAutoEnterPrivateScopeOnInteraction { get; }

        BaseUserControlWithWindow ClientOpenUI(IWorldObject worldObject);

        void ServerOnClientInteract(ICharacter who, IWorldObject worldObject);

        void ServerOnMenuClosed(ICharacter who, IWorldObject worldObject);
    }
}