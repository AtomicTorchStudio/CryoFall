namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class SkillWeaponsEnergy : ProtoSkillWeaponsRanged
    {
        public override string Description =>
            "Using energy weapons more often allows you to better understand how they work, thus improving the overall efficiency.";

        public override string Name => "Energy weapons";

        public override StatName StatNameDamageBonusMultiplier
            => StatName.WeaponEnergyDamageBonusMultiplier;

        public override StatName StatNameDegrationRateMultiplier
            => StatName.WeaponEnergyDegradationRateMultiplier;

        // no reloading stat
        public override StatName? StatNameReloadingSpeedMultiplier
            => null;

        public override StatName StatNameSpecialEffectChanceMultiplier
            => StatName.WeaponEnergySpecialEffectChanceMultiplier;

        public static double SharedGetRequiredEnergyAmount(ICharacter character, double energyUsePerShot)
        {
            var multiplier = character.SharedGetFinalStatMultiplier(
                StatName.WeaponEnergyWeaponEnergyConsumptionMultiplier);

            return energyUsePerShot * multiplier;
        }

        protected override void PrepareProtoWeaponsSkillRanged(SkillConfig config)
        {
            config.AddStatEffect(
                StatName.WeaponEnergyWeaponEnergyConsumptionMultiplier,
                formulaPercentBonus: level => -level * 2);

            base.PrepareProtoWeaponsSkillRanged(config);
        }
    }
}