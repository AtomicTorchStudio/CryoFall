namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;

    public class RateFactionJoinReturnCooldownDuration
        : BaseRateUint<RateFactionJoinReturnCooldownDuration>
    {
        public override string Description =>
            @"Applied when player attempts to join the faction back after leaving it recently.
              Please note: this value cannot be lower than Faction.JoinCooldownDuration.
              Default value: 24 hours or 86400 seconds.
              Min duration: 60 seconds. Max duration: 7 days (604800 seconds).";

        public override string Id => "Faction.JoinReturnCooldownDuration";

        public override string Name => "[Faction] Join-return cooldown duration";

        public override uint ValueDefault => 24 * 60 * 60;

        public override uint ValueMax => 7 * 24 * 60 * 60;

        public override uint ValueMin => 60;

        public override RateValueType ValueType => RateValueType.DurationSeconds;

        public override RateVisibility Visibility => RateVisibility.Advanced;

        protected override uint ServerReadValueWithRange()
        {
            var result = base.ServerReadValueWithRange();
            result = Math.Max(result, RateFactionJoinCooldownDuration.SharedValue);
            return result;
        }
    }
}