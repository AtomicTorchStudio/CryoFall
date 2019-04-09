namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class SkillWeaponsExotic : ProtoSkillWeaponsRanged
    {
        public override string Description =>
            "Utilizing exotic weapons in battle provides you with more insight into their secrets, giving you a better understanding on how to maximize their potential.";

        public override string Name => "Exotic weapons";

        public override StatName StatNameDamageBonusMultiplier
            => StatName.WeaponExoticDamageBonusMultiplier;

        public override StatName StatNameDegrationRateMultiplier
            => StatName.WeaponExoticDegradationRateMultiplier;

        public override StatName? StatNameReloadingSpeedMultiplier
            => StatName.WeaponExoticReloadingSpeedMultiplier;

        public override StatName StatNameSpecialEffectChanceMultiplier
            => StatName.WeaponExoticSpecialEffectChanceMultiplier;
    }
}