namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class BaseRateUint<TServerRate>
        : BaseRateWithRange<TServerRate, uint>
        where TServerRate : BaseRateUint<TServerRate>, new()
    {
        public override uint ValueStepChange
        {
            get
            {
                var delta = this.ValueMax - this.ValueMin;
                if (delta > 10
                    && delta % 10 >= 8) // is last digit above 8?
                {
                    // truncate
                    delta = 10 * (delta / 10);
                }

                return Math.Max(1, delta / 10);
            }
        }

        public override IViewModelRate ClientCreateViewModel()
        {
            return new ViewModelRateUint(this);
        }

        protected override uint ServerReadValueWithRange()
        {
            var description = this.DescriptionForConfigFile
                              + Environment.NewLine
                              + SharedGetAllowedRangeString(this.ValueMin.ToString(),
                                                            this.ValueMax.ToString());

            var settingValue = ServerRatesApi.Get(this.Id,
                                                  this.ValueDefault,
                                                  description);

            var clampedValue = MathHelper.Clamp(settingValue, this.ValueMin, this.ValueMax);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (settingValue == clampedValue)
            {
                return (uint)settingValue;
            }

            Api.Logger.Error(
                string.Format(
                    "Server rate value is out of range and was reset: Name={0}. Current value={1}. Allowed range: from {2} to {3}",
                    this.Id,
                    settingValue,
                    this.ValueMin,
                    this.ValueMax));

            ServerRatesApi.Reset(this.Id,
                                 this.ValueDefault,
                                 description);

            return this.ValueDefault;
        }

        protected override void SharedApplyAbstractValueToConfig(IServerRatesConfig ratesConfig, uint value)
        {
            ratesConfig.Set(this.Id,
                            value,
                            this.ValueDefault,
                            this.Description);
        }
    }
}