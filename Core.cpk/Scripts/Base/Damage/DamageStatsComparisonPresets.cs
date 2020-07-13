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
            = new DamageStatsComparisonPreset(protoAmmos: AllAvailableAmmoGrenadesOnly,
                                              protoWeapons: new Lazy<IReadOnlyList<IProtoItemWeapon>>(
                                                  Array.Empty<IProtoItemWeapon>),
                                              isRangedWeapon: false);

        public static readonly DamageStatsComparisonPreset PresetMelee
            = new DamageStatsComparisonPreset(protoAmmos: new Lazy<IReadOnlyList<IProtoItemAmmo>>(
                                                  Array.Empty<IProtoItemAmmo>),
                                              protoWeapons: AllAvailableWeaponsMelee,
                                              isRangedWeapon: false);

        public static readonly DamageStatsComparisonPreset PresetRangedExceptGrenades
            = new DamageStatsComparisonPreset(protoAmmos: AllAvailableAmmoExceptGrenades,
                                              protoWeapons: AllAvailableWeaponsRanged,
                                              isRangedWeapon: true);

        public static readonly DamageStatsComparisonPreset PresetRangedGrenades
            = new DamageStatsComparisonPreset(protoAmmos: AllAvailableAmmoGrenadesOnly,
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