namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class RatesPresetSelectionControl : BaseUserControl
    {
        public static readonly DependencyProperty CompactTilesProperty
            = DependencyProperty.Register("CompactTiles",
                                          typeof(bool),
                                          typeof(RatesPresetSelectionControl),
                                          new PropertyMetadata(default(bool)));

        public Action SelectedPresetChangedCallback;

        private Grid layoutRoot;

        private ViewModelRatesPresetSelectionControl viewModel;

        public bool CompactTiles
        {
            get => (bool)this.GetValue(CompactTilesProperty);
            set => this.SetValue(CompactTilesProperty, value);
        }

        public bool OnlyLocalServerPresets { get; set; }

        public DataEntryRatesPreset? SelectedPreset
        {
            get => this.viewModel.SelectedPreset;
            set => this.viewModel.SelectedPreset = value;
        }

        public void ResetSelectedPreset()
        {
            this.viewModel.SelectedPreset = null;
            this.viewModel.SelectedPreset = this.viewModel.RatesPresets[0];
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.layoutRoot.DataContext = this.viewModel = new ViewModelRatesPresetSelectionControl(
                                              this.OnlyLocalServerPresets,
                                              compactTileSize: this.CompactTiles,
                                              selectedPresetChanged: () => Api.SafeInvoke(
                                                                         this.SelectedPresetChangedCallback));
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}