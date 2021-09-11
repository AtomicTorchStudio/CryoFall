namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers
{
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CurrentServerRatesEditorControl : BaseUserControl
    {
        private Grid layoutRoot;

        private RatesEditorControl ratesEditorControl;

        private RatesPresetSelectionControl ratesPresetSelectionControl;

        public RatesEditorControl RatesEditor => this.ratesEditorControl;

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
            this.ratesEditorControl = this.GetByName<RatesEditorControl>("RatesEditorControl");

            this.ratesPresetSelectionControl = this.GetByName<RatesPresetSelectionControl>(
                "RatesPresetSelectionControl");
            this.ratesPresetSelectionControl.OnlyLocalServerPresets = !Api.IsEditor
                                                                      && !SharedLocalServerHelper.IsLocalServer;
            this.ratesPresetSelectionControl.SelectedPresetChangedCallback = this.ApplySelectedPreset;
        }

        protected override void OnLoaded()
        {
            this.ratesEditorControl.AnyRateChanged += this.RatesEditorControlAnyRateChangedHandler;
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.ratesEditorControl.AnyRateChanged -= this.RatesEditorControlAnyRateChangedHandler;
        }

        private void ApplySelectedPreset()
        {
            var selectedPreset = this.ratesPresetSelectionControl.SelectedPreset;
            if (selectedPreset is null)
            {
                return;
            }

            this.ratesEditorControl.AnyRateChanged -= this.RatesEditorControlAnyRateChangedHandler;
            this.ratesEditorControl.ApplyPreset(selectedPreset.Value.Preset);
            this.ratesEditorControl.AnyRateChanged += this.RatesEditorControlAnyRateChangedHandler;
        }

        private void RatesEditorControlAnyRateChangedHandler()
        {
            // reset selected preset as player customized any rate
            this.ratesPresetSelectionControl.SelectedPreset = null;
        }
    }
}