namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RateDamageFriendlyFireMultiplier
        : BaseRateDouble<RateDamageFriendlyFireMultiplier>
    {
        public override string Description =>
            @"Multiplier for the friendly fire damage i.e. when a player
              damages another friendly player (same party/faction, or an ally)
              with any weapon except explosives.
              0.0 - disable friendly fire (no damage).
              1.0 - enable friendly fire (full damage).
              You can also set it to something in between like 0.5
              to reduce the damage but not eliminate it completely.";

        public override string Id => "Damage.FriendlyFireMultiplier";

        public override string Name => "Damage by friendly players (PvP friendly fire)";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 1.0;

        public override double ValueMin => 0;

        public override double ValueStepChange => 0.1;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}