namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemKnifeIron : ProtoItemWeaponMelee
    {
        public override double DamageApplyDelay => 0.075;

        public override string Description => "Military knife. Affectionately known as pig sticker.";

        public override uint DurabilityMax => 120;

        public override double FireAnimationDuration => 0.6;

        public override string Name => "Iron knife";

        public override double SpecialEffectProbability => 0.2; // 20%

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsMelee>();

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                damageValue: 30,
                armorPiercingCoef: 0.25,
                finalDamageMultiplier: 1,
                rangeMax: 1.2,
                damageDistribution: new DamageDistribution(DamageType.Impact, 1));
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnKnifeHit(damagedCharacter, damage);
        }
    }
}