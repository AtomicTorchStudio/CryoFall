namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class BaseRateBoolean<TServerRate>
        : BaseRate<TServerRate, bool>
        where TServerRate : BaseRate<TServerRate, bool>, new()
    {
        public override IViewModelRate ClientCreateViewModel()
        {
            return new ViewModelRateBoolean(this);
        }

        protected override bool ServerReadValue()
        {
            var defaultValue = this.ValueDefault ? 1 : 0;
            var settingValue = ServerRatesApi.Get(this.Id,
                                                  defaultValue,
                                                  this.DescriptionForConfigFile);

            var clampedValue = MathHelper.Clamp(settingValue, 0, 1);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (settingValue == clampedValue)
            {
                return settingValue == 1;
            }

            Api.Logger.Error(
                string.Format(
                    "Server rate value is out of range and was reset: Name={0}. Current value={1}. Allowed range: from {2} to {3}",
                    this.Id,
                    settingValue,
                    0,
                    1));

            ServerRatesApi.Reset(this.Id,
                                 defaultValue,
                                 this.DescriptionForConfigFile);

            return this.ValueDefault;
        }

        protected override void SharedApplyAbstractValueToConfig(IServerRatesConfig ratesConfig, bool value)
        {
            ratesConfig.Set(this.Id,
                            value ? 1 : 0,
                            this.ValueDefault ? 1 : 0,
                            this.Description);
        }
    }
}