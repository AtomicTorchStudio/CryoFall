namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RateSkillExperienceGainMultiplier
        : BaseRateDouble<RateSkillExperienceGainMultiplier>
    {
        public override string Description => "Determines how quickly skills are leveled up.";

        public override string Id => "SkillExperienceGainMultiplier";

        public override string Name => "Skill XP";

        public override IRate OrderAfterRate
            => this.GetRate<RateLearningPointsGainMultiplier>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0.1;

        public override double ValueMinReasonable => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}