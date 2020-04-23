namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;

    public readonly struct AiWeaponPreset
    {
        public readonly double MaxAttackRange;

        public readonly IProtoItemWeapon ProtoWeapon;

        public AiWeaponPreset(IProtoItemWeapon protoWeapon, double maxAttackRange)
        {
            this.ProtoWeapon = protoWeapon;
            this.MaxAttackRange = Math.Min(maxAttackRange, 
                                           protoWeapon.OverrideDamageDescription.RangeMax);
        }

        public AiWeaponPreset(IProtoItemWeapon protoWeapon)
            : this(protoWeapon,
                   protoWeapon.OverrideDamageDescription.RangeMax)
        {
        }
    }
}