namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.LocalGame
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.RatesPresets;
    using AtomicTorch.CBND.CoreMod.RatesPresets.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.LocalGame.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class NewWorldControl : BaseUserControl
    {
        public static readonly DependencyProperty CommandStartNewGameProperty
            = DependencyProperty.Register("CommandStartNewGame",
                                          typeof(BaseCommand),
                                          typeof(NewWorldControl),
                                          new PropertyMetadata(default(BaseCommand)));

        private Grid layoutRoot;

        private RatesEditorControl ratesEditorControl;

        private RatesPresetSelectionControl ratesPresetSelectionControl;

        private ViewModelNewWorldControl viewModel;

        public BaseCommand CommandStartNewGame
        {
            get => (BaseCommand)this.GetValue(CommandStartNewGameProperty);
            set => this.SetValue(CommandStartNewGameProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
            this.ratesEditorControl = this.GetByName<RatesEditorControl>("RatesEditorControl");

            this.ratesPresetSelectionControl = this.GetByName<RatesPresetSelectionControl>(
                "RatesPresetSelectionControl");
            this.ratesPresetSelectionControl.OnlyLocalServerPresets = true;
            this.ratesPresetSelectionControl.SelectedPresetChangedCallback = this.ApplySelectedPreset;

            this.CommandStartNewGame = new ActionCommand(this.ExecuteCommandStartNewGame);
        }

        protected override void OnLoaded()
        {
            this.viewModel = new ViewModelNewWorldControl();
            this.layoutRoot.DataContext = this.viewModel;
            this.ratesEditorControl.AnyRateChanged += this.RatesEditorControlAnyRateChangedHandler;

            ClientTimersSystem.AddAction(0,
                                         () =>
                                         {
                                             if (!this.isLoaded)
                                             {
                                                 return;
                                             }

                                             this.ratesPresetSelectionControl.ResetSelectedPreset();
                                             var presetNormal = ClientRatesPresetsManager.RatesPresets
                                                 .FirstOrDefault(p => p.GetType()
                                                                      == typeof(RatesPresetLocalServerNormal));
                                             this.ratesPresetSelectionControl.SelectedPreset
                                                 = new DataEntryRatesPreset(presetNormal);
                                         });
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
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

        private void ExecuteCommandStartNewGame()
        {
            this.viewModel.StartNewGame(this.viewModel.SaveName, this.ratesEditorControl.GetRates());
        }

        private void RatesEditorControlAnyRateChangedHandler()
        {
            // reset selected preset as player customized any rate
            this.ratesPresetSelectionControl.SelectedPreset = null;
        }
    }
}