namespace AtomicTorch.CBND.CoreMod.Damage
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;

    public class DamageStatsComparisonPreset
    {
        public DamageStatsComparisonPreset(
            Lazy<IReadOnlyList<IProtoItemAmmo>> protoAmmos,
            Lazy<IReadOnlyList<IProtoItemWeapon>> protoWeapons,
            bool isRangedWeapon)
        {
            this.ProtoAmmos = protoAmmos;
            this.ProtoWeapons = protoWeapons;
            this.IsRangedWeapon = isRangedWeapon;
        }

        public bool IsRangedWeapon { get; }

        public Lazy<IReadOnlyList<IProtoItemAmmo>> ProtoAmmos { get; }

        public Lazy<IReadOnlyList<IProtoItemWeapon>> ProtoWeapons { get; }
    }
}