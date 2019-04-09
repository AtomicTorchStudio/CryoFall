namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.NoiseView.Data
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelWindowEditorNoiseForGround : BaseViewModel
    {
        private static readonly Color[] PresetColors =
        {
            Color.FromRgb(0x66, 0x33, 0x00),
            Color.FromRgb(0x00, 0x66, 0x00),
            Color.FromRgb(0x00, 0xAA, 0x00),
            Color.FromRgb(0x99, 0xCC, 0x00),
            Color.FromRgb(0xFF, 0xCC, 0x00),
            Color.FromRgb(0xFF, 0xFF, 0x00),
            Color.FromRgb(0xFF, 0xFF, 0xFF)
        };

        private readonly BaseCommand commandCloneNoisePreset;

        private readonly BaseCommand commandDeleteNoisePreset;

        private bool isRefreshSuppressed;

        private ushort size = 512;

        public ViewModelWindowEditorNoiseForGround()
        {
            this.commandCloneNoisePreset = new ActionCommandWithParameter(this.ExecuteCommandCloneNoisePreset);
            this.commandDeleteNoisePreset = new ActionCommandWithParameter(this.ExecuteCommandDeleteNoisePreset);
            // ReSharper disable once UseObjectOrCollectionInitializer
            this.NoisePresetViewModels = new ObservableCollection<ViewModelNoisePresetSettings>();
            this.NoisePresetViewModels.Add(new ViewModelNoisePresetSettings(
                                               new ViewModelNoiseSettings(this.Refresh,
                                                                          commandClone: null,
                                                                          commandDelete: null,
                                                                          isDisplayHeader: false,
                                                                          isDisplayCombineModeSetting: false),
                                               this.SelectColorForNextNoisePreset(),
                                               this.commandCloneNoisePreset,
                                               this.Refresh,
                                               this.commandDeleteNoisePreset));

            this.CommandAddNoise = new ActionCommand(this.ExecuteCommandAddNoise);

            this.CommandRandomize = new ActionCommand(this.ExecuteCommandRandomize);
            this.CommandRandomize.Execute(null);
        }

        public BaseCommand CommandAddNoise { get; }

        public BaseCommand CommandRandomize { get; }

        public TextureBrush GeneratedImageBrush { get; private set; }

        public ObservableCollection<ViewModelNoisePresetSettings> NoisePresetViewModels { get; }

        public ushort Size
        {
            get => this.size;
            set
            {
                value = MathHelper.Clamp(value, 16, 1024);
                if (this.size == value)
                {
                    return;
                }

                this.size = value;
                this.NotifyThisPropertyChanged();
                this.Refresh();
            }
        }

        private void CollapseAll()
        {
            foreach (var vm in this.NoisePresetViewModels)
            {
                vm.IsCollapsed = true;
            }
        }

        private void ExecuteCommandAddNoise()
        {
            this.CollapseAll();
            var noiseViewModel = new ViewModelNoiseSettings(this.Refresh,
                                                            commandClone: null,
                                                            commandDelete: null,
                                                            isDisplayHeader: false,
                                                            isDisplayCombineModeSetting: false);

            this.NoisePresetViewModels.Add(new ViewModelNoisePresetSettings(
                                               noiseViewModel,
                                               this.SelectColorForNextNoisePreset(),
                                               this.commandCloneNoisePreset,
                                               this.Refresh,
                                               this.commandDeleteNoisePreset));

            noiseViewModel.Randomize();
        }

        private void ExecuteCommandCloneNoisePreset(object obj)
        {
            this.CollapseAll();
            var original = (ViewModelNoisePresetSettings)obj;
            var clone = original.Clone();
            clone.IsCollapsed = false;

            try
            {
                this.isRefreshSuppressed = true;
                clone.Color = this.SelectColorForNextNoisePreset();
            }
            finally
            {
                this.isRefreshSuppressed = false;
            }

            this.NoisePresetViewModels.Add(clone);
            this.Refresh();
        }

        private void ExecuteCommandDeleteNoisePreset(object obj)
        {
            var viewModel = (ViewModelNoisePresetSettings)obj;
            this.NoisePresetViewModels.Remove(viewModel);
            this.Refresh();
        }

        private void ExecuteCommandRandomize()
        {
            // randomize each noise seed
            try
            {
                this.isRefreshSuppressed = true;
                foreach (var vm in this.NoisePresetViewModels)
                {
                    vm.NoiseSetttings.Randomize();
                }
            }
            finally
            {
                this.isRefreshSuppressed = false;
            }

            // refresh noise texture
            this.Refresh();
        }

        private async Task<ITextureResource> GenerateProceduralNoise(ProceduralTextureRequest request)
        {
            var presets = this.NoisePresetViewModels.Where(n => n.IsEnabled).ToArray();
            if (presets.Length == 0)
            {
                // return black texture
                var black = new Color[1, 1];
                black[0, 0] = Color.FromArgb(0xFF, 0, 0, 0);
                return await Api.Client.Rendering.CreateTextureFromColorsArray(black, 1, 1);
            }

            var noises = presets.Select(p => new PerlinNoise(p.NoiseSetttings.GetPerlinNoiseSettings()))
                                .ToArray();

            var size = this.Size;
            var colors = new Color[size, size];

            for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
            {
                Color? color = null;
                for (var index = noises.Length - 1; index >= 0; index--)
                {
                    var preset = presets[index];
                    var noiseSource = noises[index];
                    var noiseValue = noiseSource.Get(x, y);

                    if (preset.IsDebug)
                    {
                        // draw noise as is
                        var value = (byte)Math.Round(noiseValue * byte.MaxValue, MidpointRounding.AwayFromZero);
                        color = Color.FromArgb(0xFF, value, value, value);
                        break;
                    }

                    if (preset.IsNoiseValueInRange(noiseValue))
                    {
                        // preset matches - select this color
                        color = preset.Color;
                        break;
                    }
                }

                colors[x, y] = color ?? Colors.Black;
            }

            return await Api.Client.Rendering.CreateTextureFromColorsArray(colors, size, size);
        }

        private void Refresh()
        {
            if (this.isRefreshSuppressed)
            {
                return;
            }

            this.GeneratedImageBrush = Api.Client.UI.GetTextureBrush(
                new ProceduralTexture(
                    "Noise view",
                    this.GenerateProceduralNoise,
                    isTransparent: false,
                    isUseCache: false));
        }

        private Color SelectColorForNextNoisePreset()
        {
            var presetIndex = this.NoisePresetViewModels.Count;
            presetIndex = Math.Min(presetIndex, PresetColors.Length - 1);
            return PresetColors[presetIndex];
        }
    }
}