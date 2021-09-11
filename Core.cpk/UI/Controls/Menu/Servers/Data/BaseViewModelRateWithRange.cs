namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public abstract class BaseViewModelRateWithRange<TValue> : BaseViewModelRate<TValue>, IViewModelRateWithRange
        where TValue : IComparable<TValue>, IEquatable<TValue>
    {
        private readonly IRateWithRange<TValue> rate;

        private bool isExpandedRangeUnlocked;

        protected BaseViewModelRateWithRange(IRateWithRange<TValue> rate) : base(rate)
        {
            this.rate = rate;
        }

        public BaseCommand CommandToggleExpandedRange
            => new ActionCommand(this.ExecuteCommandToggleExpandedRange);

        public bool IsDurationSeconds => this.rate.ValueType == RateValueType.DurationSeconds;

        public bool IsExpandedRangeAvailable
            => !this.rate.ValueMin.Equals(this.rate.ValueMinReasonable)
               || !this.rate.ValueMax.Equals(this.rate.ValueMaxReasonable);

        public bool IsExpandedRangeUnlocked
        {
            get => this.isExpandedRangeUnlocked;
            set
            {
                if (this.isExpandedRangeUnlocked == value)
                {
                    return;
                }

                this.isExpandedRangeUnlocked = value;
                this.NotifyThisPropertyChanged();

                this.NotifyPropertyChanged(nameof(this.ValueMin));
                this.NotifyPropertyChanged(nameof(this.ValueMax));
                this.Value = Clamp(this.Value, this.ValueMin, this.ValueMax);
            }
        }

        public bool IsMultiplier => this.rate.ValueType == RateValueType.Multiplier;

        public string TextResetRange
            => string.Format("[b]{0}[/b][br]\\[{1} … {2}\\]",
                             CoreStrings.RatesEditorControl_ResetRange,
                             this.rate.ValueMinReasonable,
                             this.rate.ValueMaxReasonable);

        public string TextUnlockExpandedRange
            => string.Format("[b]{0}[/b][br]\\[{1} … {2}\\]",
                             CoreStrings.RatesEditorControl_UnlockRange,
                             this.rate.ValueMin,
                             this.rate.ValueMax);

        public override TValue Value
        {
            get => base.Value;
            set => base.Value = Clamp(value, this.ValueMin, this.ValueMax);
        }

        public TValue ValueMax
            => this.IsExpandedRangeUnlocked
                   ? this.rate.ValueMax
                   : this.rate.ValueMaxReasonable;

        public TValue ValueMin
            => this.IsExpandedRangeUnlocked
                   ? this.rate.ValueMin
                   : this.rate.ValueMinReasonable;

        public TValue ValueSliderSmallChange => this.rate.ValueStepChange;

        public override void SetAbstractValue(object value)
        {
            var wasExpandedRangeUnlocked = this.isExpandedRangeUnlocked;
            this.isExpandedRangeUnlocked = true;
            base.SetAbstractValue(value);
            this.isExpandedRangeUnlocked = wasExpandedRangeUnlocked;

            if (!this.isExpandedRangeUnlocked
                && this.IsExpandedRangeAvailable
                && (this.Value.CompareTo(this.rate.ValueMaxReasonable) > 0
                    || this.Value.CompareTo(this.rate.ValueMinReasonable) < 0))
            {
                // unlock the expanded range as the value is outside the "reasonable" range
                this.IsExpandedRangeUnlocked = true;
            }

            this.NotifyPropertyChanged(nameof(this.IsExpandedRangeUnlocked));
        }

        protected override string FormatValue(TValue x)
        {
            var result = base.FormatValue(x);
            if (this.IsMultiplier)
            {
                result = "x" + result;
            }

            if (this.IsDurationSeconds)
            {
                result += ClientTimeFormatHelper.SuffixSeconds;
            }

            return result;
        }

        private static TValue Clamp(TValue value, TValue min, TValue max)
        {
            if (value.CompareTo(min) < 0)
            {
                return min;
            }

            if (value.CompareTo(max) > 0)
            {
                return max;
            }

            return value;
        }

        private void ExecuteCommandToggleExpandedRange()
        {
            this.IsExpandedRangeUnlocked = !this.IsExpandedRangeUnlocked;
            if (this.IsExpandedRangeUnlocked)
            {
                this.Value = this.Value; // ensure the value clamping applied
            }
        }
    }
}