namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectExplosive : IProtoStaticWorldObject
    {
        bool IsActivatesRaidModeForLandClaim { get; }

        double StructureDamage { get; set; }

        double StructureDefensePenetrationCoef { get; set; }

        double ServerCalculateTotalDamageByExplosive(IProtoStaticWorldObject targetStaticWorldObjectProto);

        void ServerSetup(IStaticWorldObject worldObject, ICharacter deployedByCharacter);
    }
}