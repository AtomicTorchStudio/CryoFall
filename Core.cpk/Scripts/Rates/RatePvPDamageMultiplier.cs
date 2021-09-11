namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RatePvPDamageMultiplier
        : BaseRateDouble<RatePvPDamageMultiplier>
    {
        public override string Description =>
            @"All damage dealt from player to player (via weapons only) is multiplied by this rate.
              It allows you to decrease or increase the combat duration.
              You can set it to 0 to disable PvP damage.
              Please note: it doesn't apply to bombs damage, see DamageExplosivesToCharactersMultiplier.";

        public override string Id => "Damage.PvPMultiplier";

        public override string Name => "[PvP] Damage player to player";

        public override double ValueDefault => 0.5;

        public override double ValueMax => 5.0;

        public override double ValueMaxReasonable => 2.0;

        public override double ValueMin => 0;

        public override double ValueStepChange => 0.1;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}