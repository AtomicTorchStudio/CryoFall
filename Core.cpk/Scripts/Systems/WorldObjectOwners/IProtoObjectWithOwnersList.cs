namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectWithOwnersList : IProtoObjectStructure
    {
        bool SharedCanEditOwners(IStaticWorldObject worldObject, ICharacter byOwner);
    }
}