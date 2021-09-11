namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RateDamagePvEMultiplier
        : BaseRateDouble<RateDamagePvEMultiplier>
    {
        public override string Description =>
            @"All damage dealt from player to environment (NPC/creatures, world objects,
              also trees and rocks when player is not using a woodcutting/mining tool)
              is multiplied by this rate.
              It allows you to make it harder or easier to kill creatures by players.";

        public override string Id => "Damage.PvEMultiplier";

        public override string Name => "Damage to creatures and environment";

        public override double ValueDefault => 1;

        public override double ValueMax => 5.0;

        public override double ValueMaxReasonable => 2.0;

        public override double ValueMin => 0.1;

        public override double ValueMinReasonable => 0.5;

        public override double ValueStepChange => 0.25;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}