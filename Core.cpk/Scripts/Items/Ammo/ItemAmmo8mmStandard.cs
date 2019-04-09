namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmo8mmStandard : ProtoItemAmmo, IAmmoCaliber8mm
    {
        public override string Description =>
            "Simple 8mm non-jacketed rounds. Uses simple materials and manufacturing techniques, which leaves precision and power quite low.";

        public override string Name => "8mm standard ammo";

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 15;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1.2;
            rangeMax = 9;

            damageDistribution.Set(DamageType.Kinetic, 1);
        }
    }
}