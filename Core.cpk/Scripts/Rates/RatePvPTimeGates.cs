namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class RatePvPTimeGates
        : BaseRate<RatePvPTimeGates, string>
    {
        [NotLocalizable]
        public const string ValueNoTimeGates = "0,0,0,0,0,0,0,0";

        // This constant contains the total number of time gates
        // (depends on what are the max and min tiers for time gating).
        private const byte PvPTimeGatesCount
            = 2 * (1 + (byte)TechConstants.MaxTier - (byte)TechConstants.PvPTimeGateStartsFromTier);

        private static PvPTechTimeGateDurations sharedTimeGateDurations;

        static RatePvPTimeGates()
        {
            if (Api.IsClient)
            {
                // init placeholder value
                sharedTimeGateDurations = new PvPTechTimeGateDurations(
                    timeGates: new double[PvPTimeGatesCount]);
            }
        }

        [NotLocalizable]
        public override string Description =>
            @"Determines the time gate durations for Tier 3-6 technologies on PvP servers.
              Please configure a sequence in hours for Tier 3-6 technologies in the following order:
              T3 basic, T3 specialized,
              T4 basic, T4 specialized,
              T5 basic, T5 specialized,
              T6 basic, T6 specialized (the last two are the ""Escape"" tier).
              If you want to disable time-gating completely, please use: 0,0,0,0,0,0,0,0";

        public override string Id => "PvP.TimeGates";

        public override string Name => "[PvP] Time gates";

        public override string ValueDefault => "24,72,120,168,216,216,264,264";

        public override RateVisibility Visibility => RateVisibility.Advanced;

        public static PvPTechTimeGateDurations SharedGetTimeGateDurations()
        {
            return sharedTimeGateDurations;
        }

        public override IViewModelRate ClientCreateViewModel()
        {
            return new ViewModelRateString(this);
        }

        protected override void ClientOnValueChanged()
        {
            sharedTimeGateDurations = ParseTimeGating(SharedValue);
        }

        protected override string ServerReadValue()
        {
            var currentValue = ServerRatesApi.Get(this.Id, this.ValueDefault, this.Description);
            if (string.IsNullOrEmpty(currentValue)
                || currentValue == "0")
            {
                // most server owners will likely configure this setting to empty string or simple "0"
                // with intent to disable the time gates
                currentValue = ValueNoTimeGates;
            }

            try
            {
                sharedTimeGateDurations = ParseTimeGating(currentValue);
            }
            catch
            {
                Api.Logger.Error($"Incorrect format for server rate: {this.Id} current value {currentValue}");
                ServerRatesApi.Reset(this.Id, this.ValueDefault, this.Description);
                currentValue = this.ValueDefault;
                sharedTimeGateDurations = ParseTimeGating(currentValue);
            }

            return currentValue;
        }

        protected override void SharedApplyAbstractValueToConfig(IServerRatesConfig ratesConfig, string value)
        {
            ratesConfig.Set(this.Id,
                            value,
                            this.ValueDefault,
                            this.Description);
        }

        private static PvPTechTimeGateDurations ParseTimeGating(string str)
        {
            var split = str.Split(',');
            if (split.Length > PvPTimeGatesCount)
            {
                throw new FormatException("Incorrect number of time gates. There are "
                                          + (byte)TechConstants.MaxTier
                                          + " tiers and first two cannot have time gates."
                                          + "The rest must have properly configured time gates.");
            }

            var parsedArray = split.Select(s => int.Parse(s) * 60.0 * 60.0)
                                   .ToArray();
            return new PvPTechTimeGateDurations(parsedArray);
        }
    }
}