namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.RatesPresets.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class RatesEditorControl : BaseUserControl
    {
        private BaseRatesPreset lastPreset;

        private Grid layoutRoot;

        private ViewModelRatesEditorControl viewModel;

        public event Action AnyRateChanged;

        public void ApplyPreset(BaseRatesPreset preset)
        {
            this.lastPreset = preset;
            if (!this.isLoaded)
            {
                return;
            }

            this.viewModel.ApplyPreset(preset);
        }

        public IReadOnlyList<IViewModelRate> GetRates()
        {
            return this.viewModel.RatesAll;
        }

        public void SetRates(Dictionary<IRate, object> ratesDictionary)
        {
            this.viewModel.SetRates(ratesDictionary);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.layoutRoot.DataContext = this.viewModel = new ViewModelRatesEditorControl(
                                              () => this.AnyRateChanged?.Invoke());

            if (this.lastPreset is not null)
            {
                this.ApplyPreset(this.lastPreset);
            }
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}