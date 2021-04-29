namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectExplosive : IProtoStaticWorldObject, IProtoExplosive
    {
        DamageDescription DamageDescriptionCharacters { get; }

        bool IsActivatesRaidBlock { get; }

        void ServerSetup(IStaticWorldObject worldObject, ICharacter deployedByCharacter);
    }
}