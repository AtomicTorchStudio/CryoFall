namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectExplosive : IProtoStaticWorldObject
    {
        bool IsActivatesRaidModeForLandClaim { get; }

        double StructureDamage { get; }

        double StructureDefensePenetrationCoef { get; }

        double ServerCalculateTotalDamageByExplosive(IProtoStaticWorldObject targetStaticWorldObjectProto);

        void ServerSetup(IStaticWorldObject worldObject, ICharacter deployedByCharacter);
    }
}