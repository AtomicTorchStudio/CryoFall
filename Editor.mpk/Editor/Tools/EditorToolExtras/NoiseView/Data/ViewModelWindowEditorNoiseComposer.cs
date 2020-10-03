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

    public class ViewModelWindowEditorNoiseComposer : BaseViewModel
    {
        private readonly BaseCommand commandNoiseClone;

        private readonly BaseCommand commandNoiseDelete;

        private bool isRefreshSuppressed;

        private ushort size = 512;

        public ViewModelWindowEditorNoiseComposer()
        {
            this.commandNoiseDelete =
                new ActionCommandWithParameter(this.ExecuteCommandDeleteViewModelNoiseSettings);

            this.commandNoiseClone =
                new ActionCommandWithParameter(this.ExecuteCommandCloneViewModelNoiseSettings);

            this.NoiseViewModels = new ObservableCollection<ViewModelNoiseSettings>()
            {
                new ViewModelNoiseSettings(this.Refresh,
                                           this.commandNoiseClone,
                                           this.commandNoiseDelete,
                                           isDisplayHeader: true,
                                           isDisplayCombineModeSetting: true)
            };

            this.ViewModelNoiseViewLayers = new ViewModelNoiseLayersSettings(this.Refresh);

            this.CommandAddNoise = new ActionCommand(this.ExecuteCommandAddNoise);

            this.CommandRandomize = new ActionCommand(this.ExecuteCommandRandomize);
            this.CommandRandomize.Execute(null);
        }

        public BaseCommand CommandAddNoise { get; }

        public BaseCommand CommandRandomize { get; }

        public TextureBrush GeneratedImageBrush { get; private set; }

        public ObservableCollection<ViewModelNoiseSettings> NoiseViewModels { get; }

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

        public ViewModelNoiseLayersSettings ViewModelNoiseViewLayers { get; }

        private void CollapseAll()
        {
            foreach (var vm in this.NoiseViewModels)
            {
                vm.IsCollapsed = true;
            }
        }

        private void ExecuteCommandAddNoise()
        {
            this.CollapseAll();
            var noiseViewModel = new ViewModelNoiseSettings(this.Refresh,
                                                            this.commandNoiseClone,
                                                            this.commandNoiseDelete,
                                                            isDisplayHeader: true,
                                                            isDisplayCombineModeSetting: true);
            this.NoiseViewModels.Add(noiseViewModel);
            noiseViewModel.Randomize();
        }

        private void ExecuteCommandCloneViewModelNoiseSettings(object obj)
        {
            this.CollapseAll();
            var original = (ViewModelNoiseSettings)obj;
            var clone = original.Clone();
            clone.IsCollapsed = false;
            this.NoiseViewModels.Add(clone);
            this.Refresh();
        }

        private void ExecuteCommandDeleteViewModelNoiseSettings(object obj)
        {
            var viewModel = (ViewModelNoiseSettings)obj;
            this.NoiseViewModels.Remove(viewModel);
            this.Refresh();
        }

        private void ExecuteCommandRandomize()
        {
            // randomize each noise seed
            try
            {
                this.isRefreshSuppressed = true;
                foreach (var vm in this.NoiseViewModels)
                {
                    vm.Randomize();
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
            var noiseSource = this.GetNoiseSource();
            if (noiseSource is null)
            {
                // return black texture
                var black = new Color[1, 1];
                black[0, 0] = Color.FromArgb(0xFF, 0, 0, 0);
                return await Api.Client.Rendering.CreateTextureFromColorsArray(black, 1, 1);
            }

            var size = this.Size;
            var layers = this.ViewModelNoiseViewLayers;
            var useLayers = layers.IsLayersEnabled && layers.Layers.Count > 0;
            var colors = new Color[size, size];

            for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
            {
                var noiseValue = noiseSource.Get(x, y);

                Color color;
                if (useLayers)
                {
                    color = layers.GetColor(noiseValue);
                }
                else
                {
                    var value = (byte)Math.Round(noiseValue * byte.MaxValue, MidpointRounding.AwayFromZero);
                    color = Color.FromArgb(0xFF, value, value, value);
                }

                colors[x, y] = color;
            }

            return await Api.Client.Rendering.CreateTextureFromColorsArray(colors, size, size);
        }

        private ISimplexNoise GetNoiseSource()
        {
            var noises = this.NoiseViewModels.Where(n => n.IsEnabled).ToArray();
            if (noises.Length == 0)
            {
                return null;
            }

            if (noises.Length == 1)
            {
                return new PerlinNoise(noises[0].GetPerlinNoiseSettings());
            }

            return new CombinedNoiseSource(
                noises.Select(vm => new PerlinNoise(vm.GetPerlinNoiseSettings()))
                      .ToArray<ISimplexNoise>());
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
    }
}