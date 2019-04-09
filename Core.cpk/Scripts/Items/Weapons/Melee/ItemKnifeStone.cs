namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemKnifeStone : ProtoItemWeaponMelee
    {
        public override string Description => "Basic knife. Better than fists, but not by much.";

        public override ushort DurabilityMax => 60;

        public override string Name => "Stone knife";

        public override double SpecialEffectProbability => 0.1; // 10%

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsMelee>();

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                damageValue: 25,
                armorPiercingCoef: 0,
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