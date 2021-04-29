namespace AtomicTorch.CBND.CoreMod.Objects
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectPsiSourceCustom : IProtoObjectPsiSource
    {
        double ServerCalculatePsiIntensity(IWorldObject worldObject, ICharacter character);
    }
}