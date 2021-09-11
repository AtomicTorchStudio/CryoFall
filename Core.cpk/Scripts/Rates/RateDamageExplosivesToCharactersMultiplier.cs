namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RateDamageExplosivesToCharactersMultiplier
        : BaseRateDouble<RateDamageExplosivesToCharactersMultiplier>
    {
        public override string Description =>
            @"All damage dealt by bombs to characters is multiplied by this rate.
              You can set it to 0 to disable bomb/grenade damage to characters.
              Please note when PvE mode is set it's not possible to damage other
              characters regardless of this rate unless the duel mode is mutually 
              activated by both players.";

        public override string Id => "Damage.ExplosivesToCharactersMultiplier";

        public override string Name => "Damage by bombs to characters";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0;

        public override double ValueMinReasonable => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}