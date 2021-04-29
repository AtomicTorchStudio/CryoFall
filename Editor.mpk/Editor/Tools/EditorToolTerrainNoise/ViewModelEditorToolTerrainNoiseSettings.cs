namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrainNoise
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrain;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelEditorToolTerrainNoiseSettings : BaseViewModel
    {
        public EditorToolTerrainNoise ToolTerrain;

        private double noiseProbability = 0.1;

        public ViewModelEditorToolTerrainNoiseSettings()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.CommandApply = new ActionCommand(this.ExecuteCommandApply);
            this.CommandRandomizeSeed = new ActionCommand(this.ExecuteCommandRandomizeSeed);
            var allProtoTiles = Api.FindProtoEntities<IProtoTile>()
                                   .ExceptOne(Api.GetProtoEntity<TilePlaceholder>())
                                   .ToList();

            this.ProtoTilesForTarget = CreateList();
            this.ProtoTilesForNoise = CreateList();

            this.ExecuteCommandRandomizeSeed();

            List<ViewModelEditorToolItem> CreateList()
            {
                var list = allProtoTiles.Select(t => new ViewModelEditorToolItem(new EditorToolTerrainItem(t)))
                                        .ToList();
                foreach (var item in list)
                {
                    item.IsSelectedChanged += ViewModelIsSelectedChangeHandler;
                }

                return list;

                void ViewModelIsSelectedChangeHandler(ViewModelEditorToolItem obj)
                {
                    if (!obj.IsSelected)
                    {
                        return;
                    }

                    foreach (var item in list)
                    {
                        item.IsSelected = ReferenceEquals(item, obj);
                    }
                }
            }
        }

        public BaseCommand CommandApply { get; }

        public BaseCommand CommandRandomizeSeed { get; }

        public double NoiseProbability
        {
            get => this.noiseProbability;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (value > 1)
                {
                    value = 1;
                }

                // round to 0.01 precision
                value = (int)(value * 100) / 100.0;

                if (value == this.noiseProbability)
                {
                    return;
                }

                this.noiseProbability = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public IReadOnlyList<ViewModelEditorToolItem> ProtoTilesForNoise { get; }

        public IReadOnlyList<ViewModelEditorToolItem> ProtoTilesForTarget { get; }

        public long Seed { get; set; } = long.MaxValue;

        public IProtoTile SelectedProtoTileNoise => this.GetSelectedItem(this.ProtoTilesForNoise);

        public IProtoTile SelectedProtoTileTarget => this.GetSelectedItem(this.ProtoTilesForTarget);

        public ViewModelLocationSettings ViewModelLocationSettings { get; set; }

        private BoundsUshort SelectionBounds => this.ViewModelLocationSettings.ComponentSelectLocation.SelectionBounds;

        private void ExecuteCommandApply()
        {
            Client.UI.BlurFocus();

            try
            {
                this.ToolTerrain.Apply(
                    this.Seed,
                    this.NoiseProbability,
                    this.SelectionBounds,
                    this.SelectedProtoTileTarget,
                    this.SelectedProtoTileNoise);
            }
            catch (Exception ex)
            {
                DialogWindow.ShowMessage("There is a problem", ex.Message, closeByEscapeKey: true);
            }
        }

        private void ExecuteCommandRandomizeSeed()
        {
            this.Seed = Api.Random.Next();
        }

        private IProtoTile GetSelectedItem(IReadOnlyList<ViewModelEditorToolItem> list)
        {
            var selectedItem = list.FirstOrDefault(vm => vm.IsSelected)?.ToolItem;
            return ((EditorToolTerrainItem)selectedItem)?.ProtoTile;
        }
    }
}