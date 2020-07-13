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
            = new Lazy<IReadOnlyList<IProtoItemAmmo>>(
                () => Api.FindProtoEntities<IProtoItemAmmo>()
                         .Where(p => !(p is IAmmoGrenade))
                         .ToArray());

        public static readonly Lazy<IReadOnlyList<IProtoItemAmmo>> AllAvailableAmmoGrenadesOnly
            = new Lazy<IReadOnlyList<IProtoItemAmmo>>(
                Api.FindProtoEntities<IAmmoGrenade>);

        public static readonly Lazy<IReadOnlyList<IProtoItemWeapon>> AllAvailableWeaponsMelee
            = new Lazy<IReadOnlyList<IProtoItemWeapon>>(
                () => Process(Api.FindProtoEntities<IProtoItemWeaponMelee>()));

        public static readonly Lazy<IReadOnlyList<IProtoItemWeapon>> AllAvailableWeaponsRanged
            = new Lazy<IReadOnlyList<IProtoItemWeapon>>(
                () => Process(Api.FindProtoEntities<IProtoItemWeaponRanged>()));

        private static IReadOnlyList<IProtoItemWeapon> Process(IReadOnlyList<IProtoItemWeapon> protoWeapons)
        {
            var result = new List<IProtoItemWeapon>(capacity: protoWeapons.Count);

            foreach (var protoWeapon in protoWeapons)
            {
                switch (protoWeapon)
                {
                    case IProtoItemTool _:
                    case ProtoItemMobWeaponMelee _:
                    case ProtoItemMobWeaponRanged _:
                    case ProtoItemMobWeaponNova _:
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