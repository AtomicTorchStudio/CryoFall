namespace AtomicTorch.CBND.CoreMod.Skills
{
    public abstract class ProtoSkillWeaponsRanged : ProtoSkillWeapons
    {
        protected sealed override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryCombat>();
            this.PrepareProtoWeaponsSkillRanged(config);
        }

        protected virtual void PrepareProtoWeaponsSkillRanged(SkillConfig config)
        {
            var statNameDamageBonus = this.StatNameDamageBonusMultiplier;
            var statNameReloadingSpeed = this.StatNameReloadingSpeedMultiplier;
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

            if (statNameReloadingSpeed.HasValue)
            {
                config.AddStatEffect(
                    statNameReloadingSpeed.Value,
                    formulaPercentBonus: level => -level * 2);
            }

            config.AddStatEffect(
                statNameDegradationRate,
                formulaPercentBonus: level => -level * 2);
        }
    }
}