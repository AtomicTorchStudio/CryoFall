namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.RatesPresets.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelRatesPresetSelectionControl : BaseViewModel
    {
        private readonly Action selectedPresetChanged;

        private DataEntryRatesPreset? selectedPreset;

        public ViewModelRatesPresetSelectionControl(
            bool onlyLocalServerPresets,
            bool compactTileSize,
            Action selectedPresetChanged)
        {
            this.selectedPresetChanged = selectedPresetChanged;

            IEnumerable<BaseRatesPreset> ratesPresets = ClientRatesPresetsManager.RatesPresets;
            if (onlyLocalServerPresets)
            {
                ratesPresets = ratesPresets.Where(p => !p.IsMultiplayerOnly);
            }

            this.RatesPresets = ratesPresets
                                .Select(p => new DataEntryRatesPreset(p, compactTileSize))
                                .ToArray();
        }

        public IReadOnlyList<DataEntryRatesPreset> RatesPresets { get; }

        public DataEntryRatesPreset? SelectedPreset
        {
            get => this.selectedPreset;
            set
            {
                if (Equals(this.selectedPreset, value))
                {
                    return;
                }

                this.selectedPreset = value;
                this.NotifyThisPropertyChanged();

                this.selectedPresetChanged?.Invoke();
            }
        }
    }
}