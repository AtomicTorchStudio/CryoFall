namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class BaseRateWorldEventInterval<TWorldEvent, TServerRate> : BaseRate<TServerRate, string>
        where TServerRate : BaseRateWorldEventInterval<TWorldEvent, TServerRate>, new()
        where TWorldEvent : IProtoEntity, new()
    {
        private const string Name_Format = "World event interval — {0}";

        public static readonly Lazy<string> LazyProtoEventId
            = new(() => typeof(TWorldEvent).Name.Substring("Event".Length));

        // ReSharper disable once StaticMemberInGenericType
        public static (TimeSpan From, TimeSpan To) SharedValueIntervalHours { get; private set; }

        public abstract Interval<double> DefaultTimeIntervalHours { get; }

        [NotLocalizable]
        public override string Description => $"{LazyProtoEventId.Value} world event interval (in hours).";

        [NotLocalizable]
        public override string Id => "WorldEvent.Interval." + LazyProtoEventId.Value;

        public override string Name => string.Format(Name_Format, Api.GetProtoEntity<TWorldEvent>().Name);

        public override string ValueDefault
        {
            get
            {
                var interval = this.DefaultTimeIntervalHours;
                return $"{interval.Min:0.0##}-{interval.Max:0.0##}";
            }
        }

        public override RateVisibility Visibility => RateVisibility.Advanced;

        public override IViewModelRate ClientCreateViewModel()
        {
            return new ViewModelRateString(this);
        }

        protected override void ClientOnValueChanged()
        {
            SharedValueIntervalHours = this.ParseInterval(SharedValue);
        }

        protected override string ServerReadValue()
        {
            var currentValue = ServerRatesApi.Get(this.Id, this.ValueDefault, this.Description);

            try
            {
                SharedValueIntervalHours = this.ParseInterval(currentValue);
            }
            catch
            {
                Api.Logger.Error($"Incorrect format for server rate: {this.Id} current value {currentValue}");
                ServerRatesApi.Reset(this.Id, this.ValueDefault, this.Description);
                currentValue = this.ValueDefault;
                SharedValueIntervalHours = this.ParseInterval(currentValue);
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

        private (TimeSpan From, TimeSpan To) ParseInterval(string str)
        {
            try
            {
                TimeSpan from, to;

                if (str.IndexOf("-", StringComparison.Ordinal) > 0)
                {
                    // parse range
                    var split = str.Split('-');
                    from = TimeSpan.FromHours(double.Parse(split[0].Trim()));
                    to = TimeSpan.FromHours(double.Parse(split[1].Trim()));
                }
                else
                {
                    // parse single value
                    from = to = TimeSpan.FromHours(double.Parse(str));
                }

                if (from.TotalSeconds <= 0
                    || to.TotalSeconds <= 0
                    || to < from)
                {
                    throw new Exception("Incorrect time interval for: " + this.Id);
                }

                return (from, to);
            }
            catch (Exception ex)
            {
                Api.Logger.Exception(ex, "Cannot parse world event interval");
                var defaultInterval = this.DefaultTimeIntervalHours;
                return (TimeSpan.FromHours(defaultInterval.Min), TimeSpan.FromHours(defaultInterval.Max));
            }
        }
    }
}