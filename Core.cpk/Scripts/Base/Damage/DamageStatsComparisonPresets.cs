namespace AtomicTorch.CBND.CoreMod.Damage
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using static DamageStatsComparisonData;

    public static class DamageStatsComparisonPresets
    {
        public static readonly DamageStatsComparisonPreset PresetGrenades
            = new(protoAmmos: AllAvailableAmmoGrenadesOnly,
                  protoWeapons: new Lazy<IReadOnlyList<IProtoItemWeapon>>(
                      Array.Empty<IProtoItemWeapon>),
                  isRangedWeapon: true);

        public static readonly DamageStatsComparisonPreset PresetMelee
            = new(protoAmmos: new Lazy<IReadOnlyList<IProtoItemAmmo>>(
                      Array.Empty<IProtoItemAmmo>),
                  protoWeapons: AllAvailableWeaponsMelee,
                  isRangedWeapon: false);

        public static readonly DamageStatsComparisonPreset PresetRangedExceptGrenades
            = new(protoAmmos: AllAvailableAmmoExceptGrenades,
                  protoWeapons: AllAvailableWeaponsRanged,
                  isRangedWeapon: true);

        public static readonly DamageStatsComparisonPreset PresetRangedGrenades
            = new(protoAmmos: AllAvailableAmmoGrenadesOnly,
                  protoWeapons: AllAvailableWeaponsRanged,
                  isRangedWeapon: true);

        public static readonly IReadOnlyList<DamageStatsComparisonPreset> AllPresets
            = new[]
            {
                PresetMelee,
                PresetRangedExceptGrenades,
                PresetGrenades,
                PresetRangedGrenades
            };
    }
}