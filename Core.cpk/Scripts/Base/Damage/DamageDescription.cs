// ReSharper disable once CheckNamespace
namespace AtomicTorch.CBND.GameApi.Data.Weapons
{
    using System;
    using System.Collections.Generic;

    public class DamageDescription
    {
        public readonly double ArmorPiercingCoef;

        public readonly IReadOnlyList<DamageProportion> DamageProportions;

        public readonly double DamageValue;

        public readonly double FinalDamageMultiplier;

        public readonly double RangeMax;

        public DamageDescription(
            double damageValue,
            double armorPiercingCoef,
            double finalDamageMultiplier,
            double rangeMax,
            DamageDistribution damageDistribution)
        {
            this.ArmorPiercingCoef = armorPiercingCoef;
            this.DamageProportions = damageDistribution.ToReadOnlyList();

            if (finalDamageMultiplier < 1)
            {
                throw new ArgumentException("FinalDamageMultiplier must be >= 1", nameof(finalDamageMultiplier));
            }

            this.FinalDamageMultiplier = finalDamageMultiplier;
            this.DamageValue = damageValue;
            this.RangeMax = rangeMax;
        }
    }
}