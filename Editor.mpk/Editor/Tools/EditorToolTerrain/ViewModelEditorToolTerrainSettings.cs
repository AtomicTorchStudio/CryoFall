namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrain
{
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;

    public class ViewModelEditorToolTerrainSettings : BaseViewModelBrush
    {
        private bool isFillMode;

        public ViewModelEditorToolTerrainSettings()
        {
            this.HeightModes = EnumHelper.EnumValuesToViewModel<TerrainHeightMode>();
            this.SelectedHeightMode = this.DefaultHeightMode;
        }

        public ViewModelEnum<TerrainHeightMode> DefaultHeightMode => this.HeightModes[1];

        public ViewModelEnum<TerrainHeightMode>[] HeightModes { get; }

        public bool IsAllowTileKindChange { get; set; }

        public bool IsAllowTileProtoChangeOnlyOnTheSameHeight { get; set; }

        public bool IsApplyOnlyOnTheSameTileProto { get; set; }

        public bool IsFillMode
        {
            get => this.isFillMode;
            set
            {
                if (this.isFillMode == value)
                {
                    return;
                }

                this.isFillMode = value;
                this.NotifyThisPropertyChanged();

                if (this.isFillMode)
                {
                    this.SelectedBrushSize = 1;
                    this.SelectedBrushShape = this.BrushShapes[0];
                    this.SelectedHeightMode = this.DefaultHeightMode;
                }
            }
        }

        public ViewModelEnum<TerrainHeightMode> SelectedHeightMode { get; set; }
    }
}