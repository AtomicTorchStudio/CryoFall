namespace AtomicTorch.CBND.CoreMod.Damage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class DamageStatsComparisonData
    {
        public static readonly Lazy<IReadOnlyList<IProtoItemAmmo>> AllAvailableAmmoExceptGrenades
            = new(() => Api.FindProtoEntities<IProtoItemAmmo>()
                         .Where(p => !(p is IAmmoGrenade))
                         .ToArray());

        public static readonly Lazy<IReadOnlyList<IProtoItemAmmo>> AllAvailableAmmoGrenadesOnly
            = new(Api.FindProtoEntities<IAmmoGrenade>);

        public static readonly Lazy<IReadOnlyList<IProtoItemWeapon>> AllAvailableWeaponsMelee
            = new(() => Process(Api.FindProtoEntities<IProtoItemWeaponMelee>()));

        public static readonly Lazy<IReadOnlyList<IProtoItemWeapon>> AllAvailableWeaponsRanged
            = new(() => Process(Api.FindProtoEntities<IProtoItemWeaponRanged>()));

        private static IReadOnlyList<IProtoItemWeapon> Process(IReadOnlyList<IProtoItemWeapon> protoWeapons)
        {
            var result = new List<IProtoItemWeapon>(capacity: protoWeapons.Count);

            foreach (var protoWeapon in protoWeapons)
            {
                switch (protoWeapon)
                {
                    case IProtoItemTool:
                    case ProtoItemMobWeaponMelee:
                    case ProtoItemMobWeaponRanged:
                    case ProtoItemMobWeaponRangedNoAim:
                    case ProtoItemMobWeaponNova:
                        // ignore these
                        continue;

                    default:
                        result.Add(protoWeapon);
                        break;
                }
            }

            return result.ToArray();
        }
    }
}