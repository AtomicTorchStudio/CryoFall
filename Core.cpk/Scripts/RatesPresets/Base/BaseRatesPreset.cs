namespace AtomicTorch.CBND.CoreMod.RatesPresets.Base
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BaseRatesPreset
    {
        static BaseRatesPreset()
        {
            ClientRatesPresetsManager.EnsureInitialized();
        }

        protected BaseRatesPreset()
        {
            var thisType = this.GetType();
            var iconPath = thisType.Name.Substring("RatesPreset".Length);
            var icon = new TextureResource("RatesPresets/" + iconPath.Replace(".", "/"));
            if (!Api.Shared.IsFileExists(icon))
            {
                Api.Logger.Warning("Icon not found: " + icon.FullPath);
            }

            this.Icon = icon;
        }

        public abstract string Description { get; }

        public ITextureResource Icon { get; }

        public abstract bool IsMultiplayerOnly { get; }

        public abstract string Name { get; }

        public virtual BaseRatesPreset OrderAfterPreset => null;

        public IReadOnlyDictionary<IRate, object> Rates { get; private set; }

        public void ClientInit()
        {
            var rates = new RatesPreset();
            this.PreparePreset(rates);
            this.Rates = rates.GetDictionary();
        }

        protected TRatesPreset GetPreset<TRatesPreset>()
            where TRatesPreset : BaseRatesPreset, new()
        {
            if (Api.IsServer)
            {
                return null;
            }

            return ClientRatesPresetsManager.GetInstance<TRatesPreset>();
        }

        protected abstract void PreparePreset(RatesPreset rates);

        protected class RatesPreset
        {
            private readonly Dictionary<IRate, object> dictionary
                = new();

            public IReadOnlyDictionary<IRate, object> GetDictionary()
            {
                return this.dictionary;
            }

            public void Set<TRate, TValue>(TValue value)
                where TRate : IRate<TValue>, new()
            {
                this.dictionary[RatesManager.GetInstance<TRate>()] = value;
            }
        }
    }
}