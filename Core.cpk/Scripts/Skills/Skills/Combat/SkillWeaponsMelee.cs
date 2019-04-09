namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class SkillWeaponsMelee : ProtoSkillWeapons
    {
        public override string Description =>
            "Continuous use of melee weapons grants you prowess with this type of arm, resulting in increased damage and efficiency.";

        public override double ExperienceAddedOnKillPerMaxEnemyHealthMultiplier =>
            base.ExperienceAddedOnKillPerMaxEnemyHealthMultiplier * 2; // double exp for killing enemies

        public override double ExperienceAddedPerDamageDoneMultiplier =>
            base.ExperienceAddedPerDamageDoneMultiplier * 2; // double exp for dealing damage

        public override double ExperienceAddedPerShot =>
            0; // since melee doesn't use any ammo we don't want any exp for just waving your knife around :)

        public override string Name => "Melee weapons";

        public override StatName StatNameDamageBonusMultiplier
            => StatName.WeaponMeleeDamageBonusMultiplier;

        public override StatName StatNameDegrationRateMultiplier
            => StatName.WeaponMeleeDegradationRateMultiplier;

        public override StatName? StatNameReloadingSpeedMultiplier
            => StatName.WeaponMeleeReloadingSpeedMultiplier;

        public override StatName StatNameSpecialEffectChanceMultiplier
            => StatName.WeaponMeleeSpecialEffectChanceMultiplier;

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            // Since close combat weapons are inferior to ranged weapons it have special bonuses to counteract that.
            // (that's why we override prepare method here)

            config.Category = GetCategory<SkillCategoryCombat>();

            var statNameDamageBonus = this.StatNameDamageBonusMultiplier;
            var statNameSpecialEffectChance = this.StatNameSpecialEffectChanceMultiplier;
            var statNameDegradationRate = this.StatNameDegrationRateMultiplier;

            config.AddStatEffect(
                statNameDamageBonus,
                level: 10,
                percentBonus: 2);

            config.AddStatEffect(
                statNameDamageBonus,
                level: 15,
                percentBonus: 3);

            config.AddStatEffect(
                statNameDamageBonus,
                level: 20,
                percentBonus: 5);

            config.AddStatEffect(
                statNameSpecialEffectChance,
                formulaPercentBonus: level => level * 5);

            config.AddStatEffect(
                statNameDegradationRate,
                formulaPercentBonus: level => -level * 1);
        }
    }
}