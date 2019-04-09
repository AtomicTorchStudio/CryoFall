namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolGenerator
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelEditorToolGeneratorSettings : BaseViewModel
    {
        private readonly ObservableCollection<ViewModelHeightSetting> heightSettings;

        private BaseEditorToolGeneratorAlgorithm selectedGenerator;

        private byte startHeight = ScriptingConstants.DefaultTileHeight;

        public ViewModelEditorToolGeneratorSettings()
        {
            this.CommandApply = new ActionCommand(this.ExecuteCommandApply);
            this.CommandAddHeightSetting = new ActionCommandWithParameter(this.ExecuteCommandAddHeightSetting);
            this.CommandDeleteHeight = new ActionCommandWithParameter(this.ExecuteCommandDeleteHeight);
            this.CommandRandomizeSeed = new ActionCommand(this.ExecuteCommandRandomizeSeed);

            this.heightSettings = new ObservableCollection
                <ViewModelHeightSetting>()
                {
                    new ViewModelHeightSetting(0.5, this.CommandDeleteHeight),
                    new ViewModelHeightSetting(0.9, this.CommandDeleteHeight),
                };

            if (!IsDesignTime)
            {
                this.ExecuteCommandRandomizeSeed();
            }
        }

        public BaseCommand CommandAddHeightSetting { get; }

        public BaseCommand CommandApply { get; }

        public BaseCommand CommandDeleteHeight { get; }

        public BaseCommand CommandRandomizeSeed { get; }

        public ObservableCollection<ViewModelHeightSetting> HeightSettings => this.heightSettings;

        public bool IsCanGenerate => this.SelectionBounds.Size.LengthSquared > 0;

        public long Seed { get; set; } = long.MaxValue;

        public BaseEditorToolGeneratorAlgorithm SelectedGenerator
        {
            get => this.selectedGenerator;
            set => this.selectedGenerator = value;
        }

        public byte StartHeight
        {
            get => this.startHeight;
            set
            {
                if (value > ScriptingConstants.MaxTileHeight)
                {
                    value = ScriptingConstants.MaxTileHeight;
                }

                if (this.startHeight == value)
                {
                    return;
                }

                this.startHeight = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public ViewModelLocationSettings ViewModelLocationSettings { get; set; }

        private BoundsUshort SelectionBounds => this.ViewModelLocationSettings.ComponentSelectLocation.SelectionBounds;

        private void ExecuteCommandAddHeightSetting(object obj)
        {
            var isAddToEnd = string.Equals(obj as string, "end");
            if (isAddToEnd && this.heightSettings.Count > 0)
            {
                var maxValue = this.heightSettings.Max(s => s.MaxValue) + 0.01;
                this.heightSettings.Add(new ViewModelHeightSetting(maxValue, this.CommandDeleteHeight));
            }
            else
            {
                this.heightSettings.Insert(0, new ViewModelHeightSetting(0, this.CommandDeleteHeight));
            }
        }

        private void ExecuteCommandApply()
        {
            if (this.IsCanGenerate)
            {
                this.selectedGenerator.Setup(this.heightSettings.ToList());
                this.selectedGenerator.ClientGenerate(this.Seed, this.SelectionBounds, this.StartHeight);
            }
        }

        private void ExecuteCommandDeleteHeight(object obj)
        {
            this.heightSettings.Remove((ViewModelHeightSetting)obj);
        }

        private void ExecuteCommandRandomizeSeed()
        {
            this.Seed = Api.Random.Next();
        }
    }
}