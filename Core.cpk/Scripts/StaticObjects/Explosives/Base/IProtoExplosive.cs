namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoExplosive : IProtoGameObject
    {
        double StructureDamage { get; }

        double StructureDefensePenetrationCoef { get; }

        double ServerCalculateTotalDamageByExplosive(
            ICharacter byCharacter,
            IStaticWorldObject targetStaticWorldObject);

        void ServerOnObjectHitByExplosion(IWorldObject worldObject, double damage, WeaponFinalCache weaponCache);
    }
}