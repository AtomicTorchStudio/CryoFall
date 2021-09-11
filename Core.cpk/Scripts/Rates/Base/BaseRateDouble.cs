namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class BaseRateDouble<TServerRate>
        : BaseRateWithRange<TServerRate, double>
        where TServerRate : BaseRateWithRange<TServerRate, double>, new()
    {
        public override IRate OrderAfterRate => null;

        public override double ValueStepChange
        {
            get
            {
                var delta = (this.ValueMaxReasonable - this.ValueMinReasonable);
                if (delta < 5)
                {
                    delta /= 5;
                }
                else
                {
                    delta /= 10;
                }

                if (delta >= 0.75)
                {
                    delta = Math.Round(delta);
                }

                return delta;
            }
        }

        public override IViewModelRate ClientCreateViewModel()
        {
            return new ViewModelRateDouble(this);
        }

        protected override double ServerReadValueWithRange()
        {
            var description = this.DescriptionForConfigFile
                              + Environment.NewLine
                              + SharedGetAllowedRangeString(this.ValueMin.ToString("0.###"),
                                                            this.ValueMax.ToString("0.###"));

            var settingValue = ServerRatesApi.Get(this.Id,
                                                  this.ValueDefault,
                                                  description);

            var clampedValue = MathHelper.Clamp(settingValue, this.ValueMin, this.ValueMax);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (settingValue == clampedValue)
            {
                return settingValue;
            }

            Api.Logger.Error(
                string.Format(
                    "Server rate value is out of range and was reset: Name={0}. Current value={1:0.###}. Allowed range: from {2:0.###} to {3:0.###}",
                    this.Id,
                    settingValue,
                    this.ValueMin,
                    this.ValueMax));

            ServerRatesApi.Reset(this.Id,
                                 this.ValueDefault,
                                 description);

            return this.ValueDefault;
        }

        protected override void SharedApplyAbstractValueToConfig(IServerRatesConfig ratesConfig, double value)
        {
            ratesConfig.Set(this.Id,
                            value,
                            this.ValueDefault,
                            this.Description);
        }
    }
}