namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners
{
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectWithOwnersList : IInteractableProtoWorldObject
    {
        bool HasOwnersList { get; }

        bool SharedCanEditOwners(IWorldObject worldObject, ICharacter byOwner);
    }
}