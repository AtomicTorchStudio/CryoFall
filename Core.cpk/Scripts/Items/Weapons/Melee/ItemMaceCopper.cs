namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemMaceCopper : ProtoItemWeaponMelee
    {
        public override double DamageApplyDelay => 0.1;

        public override string Description => "Wooden mace with copper-reinforced impact surface.";

        public override ushort DurabilityMax => 120;

        public override double FireAnimationDuration => 0.8;

        public override string Name => "Copper mace";

        public override double SpecialEffectProbability => 0.06; // 6%

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsMelee>();

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                damageValue: 25,
                armorPiercingCoef: 0.4,
                finalDamageMultiplier: 1.15,
                rangeMax: 1.2,
                damageDistribution: new DamageDistribution(DamageType.Impact, 1));
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnMaceHit(damagedCharacter, damage);
        }
    }
}