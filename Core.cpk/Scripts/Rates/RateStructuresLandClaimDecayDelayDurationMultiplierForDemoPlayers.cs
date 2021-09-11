namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RateStructuresLandClaimDecayDelayDurationMultiplierForDemoPlayers
        : BaseRateDouble<RateStructuresLandClaimDecayDelayDurationMultiplierForDemoPlayers>
    {
        [NotLocalizable]
        public override string Description =>
            @"If you host just a simple community server, please ignore this setting.
              (it's for demo players only, applies ONLY to the official and featured servers,
              as demo players cannot connect to any other community servers)";

        public override string Id => "Structures.LandClaimDecay.DelayDurationMultiplierForDemoPlayers";

        [NotLocalizable] // this is a hidden setting as it's useful only on the official servers
        public override string Name
            => "[Decay] Abandoned land claim decay delay duration - for demo players only";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100;

        public override double ValueMin => 0.5;

        public override RateValueType ValueType => RateValueType.Multiplier;

        /// <summary>
        /// This rate is not useful for community servers.
        /// </summary>
        public override RateVisibility Visibility => RateVisibility.Hidden;

        protected override double ServerReadValueWithRange()
        {
            var result = base.ServerReadValueWithRange();
            if (result
                > RateStructuresLandClaimDecayDelayDurationMultiplier.SharedValue)
            {
                result = RateStructuresLandClaimDecayDelayDurationMultiplier.SharedValue;
                Api.Logger.Error(
                    string.Format(
                        "Please note: {0} server rate value is higher than {1} which is not correct (it must be lower or equal).",
                        this.Id,
                        RatesManager
                            .GetInstance<RateStructuresLandClaimDecayDelayDurationMultiplier>().Id));
            }

            return result;
        }
    }
}