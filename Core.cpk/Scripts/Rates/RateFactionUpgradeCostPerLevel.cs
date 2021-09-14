namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class RateFactionUpgradeCostPerLevel
        : BaseRate<RateFactionUpgradeCostPerLevel, string>
    {
        public static ushort[] SharedFactionUpgradeCosts { get; private set; }

        [NotLocalizable]
        public override string Description =>
            @"Determines the upgrade cost (in LP) for each faction level increase.
              Please note: the max faction level is 10 and the first one is received automatically,
              so this setting must contain 9 comma-separated values (corresponding to each level).
              Max value per level: 65535";

        public override string Id => "Faction.UpgradeCostPerLevel";

        public override string Name => "[Faction] Level upgrade cost per level";

        public override string ValueDefault => "200,500,1000,1700,2500,3500,5000,7000,10000";

        public override RateVisibility Visibility => RateVisibility.Advanced;

        public override IViewModelRate ClientCreateViewModel()
        {
            return new ViewModelRateString(this);
        }

        protected override void ClientOnValueChanged()
        {
            SharedFactionUpgradeCosts = ParseFactionUpgradeCosts(SharedValue);
        }

        protected override string ServerReadValue()
        {
            var currentValue = ServerRatesApi.Get(this.Id, this.ValueDefault, this.Description);

            try
            {
                SharedFactionUpgradeCosts = ParseFactionUpgradeCosts(currentValue);
            }
            catch
            {
                Api.Logger.Error(
                    $"Incorrect format for server rate: {this.Id} current value {currentValue}. Please note that the values must be separated by comma and each value must be NOT higher than 65535.");
                ServerRatesApi.Reset(this.Id, this.ValueDefault, this.Description);
                currentValue = this.ValueDefault;
                SharedFactionUpgradeCosts = ParseFactionUpgradeCosts(currentValue);
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

        private static ushort[] ParseFactionUpgradeCosts(string str)
        {
            var split = str.Split(',');
            if (split.Length != FactionConstants.MaxFactionLevel - 1)
            {
                throw new FormatException();
            }

            var result = new ushort[FactionConstants.MaxFactionLevel - 1];
            for (var index = 0; index < split.Length; index++)
            {
                var entry = split[index];
                result[index] = ushort.Parse(entry);
            }

            return result;
        }
    }
}