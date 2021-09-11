namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.Systems.PvE;

    public class RateDamageExplosivesToStructuresMultiplier
        : BaseRateDouble<RateDamageExplosivesToStructuresMultiplier>
    {
        public override string Description =>
            @"All damage dealt by bombs and grenades to structures is multiplied by this rate.
              You can set it to 0 to disable explosives damage to structures.
              Applies only on PvP servers, on PvE it will always be 0.";

        public override string Id => "Damage.ExplosivesToStructuresMultiplier";

        public override string Name => "Damage by bombs to structures";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0;

        public override double ValueMinReasonable => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;

        protected override double ServerReadValueWithRange()
        {
            var result = base.ServerReadValueWithRange();
            if (PveSystem.ServerIsPvE)
            {
                result = 0;
            }

            return result;
        }
    }
}