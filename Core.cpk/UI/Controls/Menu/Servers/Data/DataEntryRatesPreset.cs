namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.RatesPresets.Base;
    using AtomicTorch.CBND.GameApi.Scripting;

    public readonly struct DataEntryRatesPreset : IEquatable<DataEntryRatesPreset>
    {
        public DataEntryRatesPreset(BaseRatesPreset preset, bool compactSize)
        {
            this.Preset = preset;
            this.CompactSize = compactSize;
        }

        public bool CompactSize { get; }

        public string Description => this.Preset.Description;

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.Preset.Icon);

        public string Name => this.Preset.Name;

        public BaseRatesPreset Preset { get; }

        public bool Equals(DataEntryRatesPreset other)
        {
            return Equals(this.Preset, other.Preset);
        }

        public override bool Equals(object obj)
        {
            return obj is DataEntryRatesPreset other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return (this.Preset != null ? this.Preset.GetHashCode() : 0);
        }
    }
}