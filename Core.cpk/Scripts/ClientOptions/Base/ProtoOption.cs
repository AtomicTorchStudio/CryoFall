namespace AtomicTorch.CBND.CoreMod.ClientOptions
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    [PrepareOrder(afterType: typeof(ProtoOptionsCategory))]
    public abstract class ProtoOption<TProtoOptionsCategory, TValue> : ProtoEntity, IProtoOption
        where TProtoOptionsCategory : ProtoOptionsCategory, new()
    {
        private TProtoOptionsCategory category;

        private bool isCurrentValueInitialized;

        private OptionValueHolder optionValueHolder;

        public TProtoOptionsCategory Category => this.category;

        public TValue CurrentValue
        {
            get => this.optionValueHolder.Value;
            set
            {
                value = this.ClampValue(value);

                if (value.Equals(this.optionValueHolder.Value)
                    && this.isCurrentValueInitialized)
                {
                    return;
                }

                this.isCurrentValueInitialized = true;
                this.optionValueHolder.IsValueModificationFromUI = false;
                this.optionValueHolder.Value = value;
                this.optionValueHolder.IsValueModificationFromUI = true;

                this.OnCurrentValueChanged(fromUi: false);
            }
        }

        public abstract TValue Default { get; }

        public virtual bool IsHidden => false;

        public bool IsModified => !this.CurrentValue.Equals(this.SavedValue);

        public virtual IProtoOption OrderAfterOption { get; }

        public abstract TValue ValueProvider { get; set; }

        protected internal OptionValueHolder InternalOptionValueHolder => this.optionValueHolder;

        protected TValue SavedValue { get; private set; }

        public void Apply()
        {
            this.SavedValue = this.ValueProvider = this.CurrentValue;
            //this.OnApply();
        }

        public virtual void ApplyAbstractValue(object value)
        {
            if (value is TValue casted)
            {
                this.CurrentValue = casted;
                this.Apply();
                return;
            }

            Logger.Warning(
                $"Option {this.Name} cannot apply abstract value - type mismatch. Will reset option to the default value");
            this.Reset(apply: true);
        }

        public void Cancel()
        {
            this.CurrentValue = this.SavedValue;
            //this.OnCancel();
        }

        public void CreateControl(out FrameworkElement labelControl, out FrameworkElement optionControl)
        {
            this.CreateControlInternal(out labelControl, out optionControl);
        }

        public virtual object GetAbstractValue()
        {
            return this.CurrentValue;
        }

        public abstract void RegisterValueType(IClientStorage storage);

        public virtual void Reset(bool apply)
        {
            this.CurrentValue = this.Default;
            if (apply)
            {
                this.Apply();
            }
        }

        public virtual TValue RoundValue(TValue value)
        {
            return value;
        }

        protected virtual TValue ClampValue(TValue value)
        {
            return value;
        }

        protected abstract void CreateControlInternal(
            out FrameworkElement labelControl,
            out FrameworkElement optionControl);

        protected virtual void OnCurrentValueChanged(bool fromUi)
        {
        }

        protected override void PrepareProto()
        {
            if (IsServer)
            {
                return;
            }

            this.category = GetProtoEntity<TProtoOptionsCategory>();
            this.category.RegisterOption(this);

            this.optionValueHolder = new OptionValueHolder(this, this.Default);
        }

        protected void SetupOptionToControlValueBinding(FrameworkElement control, DependencyProperty valueProperty)
        {
            control.SetBinding(valueProperty, "Value");
            control.DataContext = this.optionValueHolder;
        }

        /// <summary>
        /// Option value holder is used for data binding between UI control and the option.
        /// </summary>
        protected internal class OptionValueHolder : INotifyPropertyChanged
        {
            internal bool IsValueModificationFromUI = true;

            private readonly ProtoOption<TProtoOptionsCategory, TValue> owner;

            private TValue value;

            public OptionValueHolder(ProtoOption<TProtoOptionsCategory, TValue> owner, TValue initialValue)
            {
                this.owner = owner;
                this.value = initialValue;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public TValue Value
            {
                get => this.value;
                set
                {
                    value = this.owner.RoundValue(value);
                    if (EqualityComparer<TValue>.Default.Equals(value, this.value))
                    {
                        return;
                    }

                    //Logger.WriteDev("Value changed: " + value);
                    this.value = value;

                    // call property changed notification to notify UI about the change
                    this.PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(this.Value)));

                    if (this.IsValueModificationFromUI)
                    {
                        this.owner.OnCurrentValueChanged(fromUi: true);
                    }

                    this.owner.category.OnOptionModified(this.owner);
                }
            }
        }
    }
}