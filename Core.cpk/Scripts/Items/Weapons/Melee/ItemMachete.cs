namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemMachete : ProtoItemWeaponMelee
    {
        public override string Description => "Heavy steel machete. Ideal for chopping heads off.";

        public override uint DurabilityMax => 180;

        public override string Name => "Machete";

        public override double SpecialEffectProbability => 0.3; // 30%

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsMelee>();

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                damageValue: 32,
                armorPiercingCoef: 0.25,
                finalDamageMultiplier: 1.2,
                rangeMax: 1.2,
                damageDistribution: new DamageDistribution(DamageType.Impact, 1));
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnKnifeHit(damagedCharacter, damage);
        }
    }
}