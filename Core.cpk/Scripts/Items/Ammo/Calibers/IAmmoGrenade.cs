namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public interface IAmmoGrenade : IProtoItemAmmo, IProtoExplosive
    {
        DamageDescription DamageDescriptionCharacters { get; }

        double DamageRadius { get; }

        ExplosionPreset ExplosionPreset { get; }

        double FireRangeMax { get; }

        double StructureDamage { get; }

        double StructureDefensePenetrationCoef { get; }
    }
}