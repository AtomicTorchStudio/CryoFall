﻿namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using System.Collections.Generic;

    public class ItemAmmoPaperCartridge : ProtoItemAmmo, IAmmoPaperCartrige
    {
        public override string Description =>
            "Basic firearm ammunition. Suitable only against weaker targets, but cheap and easy to produce.";

        public override bool IsReferenceAmmo => true;

        public override string Name => "Paper cartridge";

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 20;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1.5;
            rangeMax = 8;
            damageDistribution.Set(DamageType.Kinetic, 0.7)
                              .Set(DamageType.Impact, 0.3);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Blackpowder;
        }

        protected override void PrepareHints(List<string> hints)
        {
            // no hints for this ammo caliber since there is only one type
        }
    }
}