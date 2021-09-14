namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateFactionJoinCooldownDuration
        : BaseRateUint<RateFactionJoinCooldownDuration>
    {
        [NotLocalizable]
        public override string Description =>
            @"Faction switch cooldown duration (in seconds).
              Applied when leaving a faction so player cannot join or create another faction quickly.                          
              Default value: 6 hours or 21600 seconds.
              Min duration: 60 seconds. Max duration: 7 days (604800 seconds).";

        public override string Id => "Faction.JoinCooldownDuration";

        public override string Name => "[Faction] Join cooldown duration";

        public override uint ValueDefault => 6 * 60 * 60; // 6 hours

        public override uint ValueMax => 7 * 24 * 60 * 60; // 7 days

        public override uint ValueMin => 60; // 60 seconds

        public override RateValueType ValueType => RateValueType.DurationSeconds;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}