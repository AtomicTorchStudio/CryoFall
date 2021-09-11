namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public abstract class BaseRate<TServerRate, TValue> : IRate<TValue>
        where TServerRate : BaseRate<TServerRate, TValue>, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static bool sharedIsReady;

        // ReSharper disable once StaticMemberInGenericType
        private static TValue sharedValue;

        protected BaseRate()
        {
            if (this.GetType() != typeof(TServerRate))
            {
                Api.Logger.Error("Incorrect server rate class - please check that its generic argument matches self: "
                                 + this.GetType().FullName);
            }
        }

        public static event Action ClientValueChanged;

        public static bool ClientIsValueReceived => sharedIsReady;

        public static TValue SharedValue
        {
            get
            {
                if (sharedIsReady)
                {
                    return sharedValue;
                }

                return InternalGetValueSlow(logErrorIfClientHasNoValue: true);
            }
        }

        public object AbstractValueDefault => this.ValueDefault;

        [NotLocalizable]
        public abstract string Description { get; }

        [NotLocalizable]
        public abstract string Id { get; }

        public abstract string Name { get; }

        public virtual IRate OrderAfterRate => null;

        public object SharedAbstractValue => SharedValue;

        public abstract TValue ValueDefault { get; }

        public abstract RateVisibility Visibility { get; }

        TValue IRate<TValue>.SharedValue => SharedValue;

        protected string DescriptionForConfigFile
        {
            get
            {
                if (this.Name == this.Id)
                {
                    return this.Description;
                }

                return this.Name
                       + Environment.NewLine
                       + this.Description;
            }
        }

        public static TValue GetSharedValue(bool logErrorIfClientHasNoValue = true)
        {
            if (sharedIsReady)
            {
                return sharedValue;
            }

            return InternalGetValueSlow(logErrorIfClientHasNoValue);
        }

        public abstract IViewModelRate ClientCreateViewModel();

        public void ClientSetAbstractValue(object value)
        {
            sharedIsReady = true;
            sharedValue = (TValue)value;
            Api.Logger.Important($"Received server rate: {this.Id}={sharedValue}");
            this.ClientOnValueChanged();
            Api.SafeInvoke(() => ClientValueChanged?.Invoke());
        }

        public void ServerInit()
        {
            if (sharedIsReady)
            {
                return;
            }

            Api.ValidateIsServer();
            sharedValue = this.ServerReadValue();
            sharedIsReady = true;
            Api.Logger.Important($"Initialized server rate: {this.Id}={sharedValue}");
        }

        public void SharedApplyToConfig(IServerRatesConfig ratesConfig, object value)
        {
            if (typeof(TValue) == typeof(bool)
                && value is int x)
            {
                value = x == 1;
            }

            if (value is TValue convertedValue)
            {
                this.SharedApplyAbstractValueToConfig(ratesConfig, convertedValue);
                return;
            }

            try
            {
                convertedValue = (TValue)Convert.ChangeType(value, typeof(TValue));
                this.SharedApplyAbstractValueToConfig(ratesConfig, convertedValue);
                return;
            }
            catch
            {
                // cannot convert
            }

            Api.Logger.Error("Incorrect value type " + value + " " + value?.GetType().FullName + " for " + this);
        }

        protected virtual void ClientOnValueChanged()
        {
        }

        protected TRate GetRate<TRate>()
            where TRate : IRate, new()
        {
            return RatesManager.GetInstance<TRate>();
        }

        protected abstract TValue ServerReadValue();

        protected abstract void SharedApplyAbstractValueToConfig(IServerRatesConfig ratesConfig, TValue value);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TValue InternalGetValueSlow(bool logErrorIfClientHasNoValue)
        {
            if (Api.IsClient)
            {
                var fallbackValue = RatesManager.GetInstance<TServerRate>()
                                                .ValueDefault;
                if (!logErrorIfClientHasNoValue)
                {
                    return fallbackValue;
                }

                Api.Logger.Error("The server rate value is not yet received from the server: "
                                 + typeof(TServerRate)
                                 + " - will provide the fallback value: "
                                 + fallbackValue);
                return fallbackValue;
            }

            // Invoking this static method will ensure that all the static constructors are invoked
            // that will in turn initialize all the server rates (including the current one).
            RatesManager.SharedEnsureInitialized();

            if (!sharedIsReady)
            {
                // apparently due to recursion the rate was not initialized - initialize it now
                RatesManager.GetInstance<TServerRate>()
                            .ServerInit();
            }

            return sharedValue;
        }
    }
}