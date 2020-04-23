namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectExplosive : IProtoStaticWorldObject, IProtoExplosive
    {
        bool IsActivatesRaidModeForLandClaim { get; }

        void ServerSetup(IStaticWorldObject worldObject, ICharacter deployedByCharacter);
    }
}