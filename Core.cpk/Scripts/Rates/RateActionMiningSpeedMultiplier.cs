﻿namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateActionMiningSpeedMultiplier
        : BaseRateDouble<RateActionMiningSpeedMultiplier>
    {
        [NotLocalizable]
        public override string Description =>
            "Adjusts the damage to minerals by tools and drones.";

        public override string Id => "Action.MiningSpeedMultiplier";

        public override string Name => "Mining speed";

        public override IRate OrderAfterRate
            => this.GetRate<RateActionWoodcuttingSpeedMultiplier>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}