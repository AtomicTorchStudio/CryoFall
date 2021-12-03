namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CurrentServerRatesEditorControl : BaseUserControl
    {
        public static readonly DependencyProperty CompactTilesProperty
            = DependencyProperty.Register("CompactTiles",
                                          typeof(bool),
                                          typeof(CurrentServerRatesEditorControl),
                                          new PropertyMetadata(default(bool)));

        private Grid layoutRoot;

        private RatesEditorControl ratesEditorControl;

        private RatesPresetSelectionControl ratesPresetSelectionControl;

        public bool CompactTiles
        {
            get => (bool)this.GetValue(CompactTilesProperty);
            set => this.SetValue(CompactTilesProperty, value);
        }

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
            this.ratesPresetSelectionControl.CompactTiles = this.CompactTiles;
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