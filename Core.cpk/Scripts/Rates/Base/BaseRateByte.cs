namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class BaseRateByte<TServerRate>
        : BaseRateWithRange<TServerRate, byte>
        where TServerRate : BaseRateByte<TServerRate>, new()
    {
        public override byte ValueStepChange => 1;

        public override IViewModelRate ClientCreateViewModel()
        {
            return new ViewModelRateByte(this);
        }

        protected override byte ServerReadValueWithRange()
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
                return (byte)settingValue;
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

        protected override void SharedApplyAbstractValueToConfig(IServerRatesConfig ratesConfig, byte value)
        {
            ratesConfig.Set(this.Id,
                            value,
                            this.ValueDefault,
                            this.Description);
        }
    }
}