namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.NoiseView.Data
{
    using System;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelNoiseSettings : BaseViewModel
    {
        private readonly Action callbackRefresh;

        private bool isEnabled = true;

        private double lacunarity = 2;

        private int octaves = 6;

        private double persistance = 0.6;

        private double scale = 150;

        private int seed;

        private ViewModelEnum<NoiseCombineMode> selectedCombineMode;

        public ViewModelNoiseSettings(
            Action callbackRefresh,
            BaseCommand commandClone,
            BaseCommand commandDelete,
            bool isDisplayHeader,
            bool isDisplayCombineModeSetting)
        {
            this.CombineModes = EnumHelper.EnumValuesToViewModel<NoiseCombineMode>();
            this.SelectedCombineMode = this.CombineModes[0];

            this.callbackRefresh = callbackRefresh;
            this.CommandDelete = commandDelete;
            this.CommandClone = commandClone;
            this.IsDisplayHeader = isDisplayHeader;
            this.IsDisplayCombineModeSetting = isDisplayCombineModeSetting;
        }

        public ViewModelEnum<NoiseCombineMode>[] CombineModes { get; }

        public BaseCommand CommandClone { get; }

        public BaseCommand CommandCollapse => new ActionCommand(this.ExecuteCommandCollapse);

        public BaseCommand CommandCopyCode => new ActionCommand(this.ExecuteCommandCopyCode);

        public BaseCommand CommandDelete { get; }

        public BaseCommand CommandPasteCode => new ActionCommand(this.ExecuteCommandPasteCode);

        public bool IsCollapsed { get; set; }

        public bool IsDisplayCombineModeSetting { get; }

        public bool IsDisplayHeader { get; }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }

                this.isEnabled = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        public double Lacunarity
        {
            get => this.lacunarity;
            set
            {
                value = Math.Round(value, digits: 2);
                if (this.lacunarity == value)
                {
                    return;
                }

                this.lacunarity = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        public int Octaves
        {
            get => this.octaves;
            set
            {
                value = MathHelper.Clamp(value, 1, 7);
                if (this.octaves == value)
                {
                    return;
                }

                this.octaves = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        public double Persistance
        {
            get => this.persistance;
            set
            {
                value = Math.Round(value, digits: 1);
                if (this.persistance == value)
                {
                    return;
                }

                this.persistance = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        public double Scale
        {
            get => this.scale;
            set
            {
                value = MathHelper.Clamp(value, 1, 500);
                value = Math.Round(value, digits: 1);
                if (this.scale == value)
                {
                    return;
                }

                this.scale = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        public int Seed
        {
            get => this.seed;
            set
            {
                if (this.seed == value)
                {
                    return;
                }

                this.seed = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        public ViewModelEnum<NoiseCombineMode> SelectedCombineMode
        {
            get => this.selectedCombineMode;
            set
            {
                if (this.selectedCombineMode == value)
                {
                    return;
                }

                this.selectedCombineMode = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        public ViewModelNoiseSettings Clone()
        {
            return (ViewModelNoiseSettings)this.MemberwiseClone();
        }

        public string GetNoiseCode()
        {
            return new StringBuilder($"new {nameof(PerlinNoise)}(", capacity: 512)
                   .Append(this.GetPerlinNoiseSettings()
                               .GetSettingsCode())
                   .Append(")")
                   .ToString();
        }

        public PerlinNoiseSettings GetPerlinNoiseSettings()
        {
            return new(this.Seed,
                       this.Scale,
                       this.Octaves,
                       this.Persistance,
                       this.Lacunarity,
                       this.SelectedCombineMode.Value);
        }

        public void Randomize()
        {
            this.Seed = RandomHelper.Next();
        }

        private void ExecuteCommandCollapse()
        {
            this.IsCollapsed = !this.IsCollapsed;
        }

        private void ExecuteCommandCopyCode()
        {
            Api.Client.Core.CopyToClipboard(this.GetNoiseCode());
        }

        private void ExecuteCommandPasteCode()
        {
            try
            {
                var text = Api.Client.Core.GetClipboard();
                var settings = PerlinNoiseSettings.Parse(text);
                this.Seed = settings.Seed;
                this.Scale = settings.Scale;
                this.Octaves = settings.Octaves;
                this.Persistance = settings.Persistance;
                this.Lacunarity = settings.Lacunarity;
                //this.SelectedCombineMode = CombineModes.FirstOrDefault(m => m.Value == settings.CombineMode);
            }
            catch (Exception ex)
            {
                DialogWindow.ShowMessage("Cannot parse code",
                                         "There is a problem: " + ex.Message,
                                         closeByEscapeKey: true);
            }
        }

        private void Refresh()
        {
            this.callbackRefresh?.Invoke();
        }
    }
}