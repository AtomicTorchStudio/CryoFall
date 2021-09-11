namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using System.Globalization;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public abstract class BaseViewModelRate<TValue> : BaseViewModel, IViewModelRate
        where TValue : IComparable<TValue>
    {
        protected readonly IRate<TValue> rate;

        private TValue value;

        protected BaseViewModelRate(IRate<TValue> rate)
        {
            this.rate = rate;
            this.value = this.rate.ValueDefault;
        }

        public BaseCommand CommandSetDefault
            => new ActionCommand(this.ResetToDefault);

        public string Id => this.rate.Id;

        public bool IsDefaultValue
            => Equals(this.value, this.rate.ValueDefault);

        public string Name
            => this.rate.Name.ToUpper(CultureInfo.CurrentUICulture);

        public IRate Rate => this.rate;

        public virtual TValue Value
        {
            get => this.value;
            set
            {
                if (Equals(this.value, value))
                {
                    return;
                }

                this.value = value;

                //Logger.Dev($"UI rate value changed: {this.rate.Id}={value}");

                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.IsDefaultValue));
                this.NotifyPropertyChanged(nameof(this.ValueText));

                this.ValueChangedCallback?.Invoke();
            }
        }

        public Action ValueChangedCallback { get; set; }

        public TValue ValueDefault => this.rate.ValueDefault;

        public string ValueDefaultText
        {
            get
            {
                if (typeof(TValue) == typeof(bool))
                {
                    return Convert.ToBoolean(this.ValueDefault)
                               ? CoreStrings.TitleEnabled
                               : CoreStrings.TitleDisabled;
                }

                return this.FormatValue(this.ValueDefault);
            }
        }

        public string ValueText
        {
            get
            {
                if (typeof(TValue) == typeof(bool))
                {
                    return Convert.ToBoolean(this.value)
                               ? CoreStrings.TitleEnabled
                               : CoreStrings.TitleDisabled;
                }

                return this.FormatValue(this.value);
            }
        }

        object IViewModelRate.ValueDefault => this.ValueDefault;

        public object GetAbstractValue()
        {
            return this.Value;
        }

        public void ResetToDefault()
        {
            this.Value = this.rate.ValueDefault;
        }

        public virtual void SetAbstractValue(object value)
        {
            this.Value = (TValue)value;
        }

        protected virtual string FormatValue(TValue x)
        {
            if (x is double valueDouble)
            {
                return valueDouble.ToString("0.##");
            }

            return x.ToString();
        }
    }
}