namespace AtomicTorch.CBND.CoreMod.Damage
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public static class DamageStatsComparisonHelper
    {
        public static readonly IReadOnlyDictionary<DamageStatsComparisonPreset, DamageDescription> MaxPresets;

        static DamageStatsComparisonHelper()
        {
            var dictionary = new Dictionary<DamageStatsComparisonPreset, DamageDescription>();
            foreach (var preset in DamageStatsComparisonPresets.AllPresets)
            {
                dictionary[preset] = GetValuesPreset(preset.ProtoAmmos.Value,
                                                     preset.ProtoWeapons.Value,
                                                     defaultValue: 0);
            }

            MaxPresets = dictionary;
        }

        private static DamageDescription GetValuesPreset(
            IReadOnlyList<IProtoItemAmmo> protoAmmos,
            IReadOnlyList<IProtoItemWeapon> protoWeapons,
            double defaultValue)
        {
            double damage = defaultValue,
                   stoppingPower = defaultValue,
                   range = defaultValue;

            foreach (var protoAmmo in protoAmmos)
            {
                var d = protoAmmo.DamageDescription;
                if (d.DamageValue == 0)
                {
                    continue;
                }

                foreach (var protoWeapon in protoAmmo.CompatibleWeaponProtos)
                {
                    Process(d,
                            protoWeapon.DamageMultiplier,
                            protoWeapon.RangeMultiplier);
                }
            }

            foreach (var protoWeapon in protoWeapons)
            {
                var d = protoWeapon.OverrideDamageDescription;
                if (d?.DamageValue > 0)
                {
                    Process(d,
                            protoWeapon.DamageMultiplier,
                            protoWeapon.RangeMultiplier);
                }
            }

            return new DamageDescription(damageValue: damage,
                                         armorPiercingCoef: 1,
                                         finalDamageMultiplier: stoppingPower,
                                         range,
                                         damageDistribution: new DamageDistribution());

            void Process(DamageDescription d, double damageMultiplier, double rangeMultiplier)
            {
                var dDamage = d.DamageValue * damageMultiplier;
                if (damage < dDamage)
                {
                    damage = dDamage;
                }

                if (stoppingPower < d.FinalDamageMultiplier)
                {
                    stoppingPower = d.FinalDamageMultiplier;
                }

                var dRange = d.RangeMax * rangeMultiplier;
                if (range < dRange)
                {
                    range = dRange;
                }
            }
        }
    }
}