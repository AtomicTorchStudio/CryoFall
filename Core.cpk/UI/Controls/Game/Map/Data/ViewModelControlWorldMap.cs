namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelControlWorldMap : BaseViewModel
    {
        public string CurrentPositionText { get; set; }

        public byte MapExploredPercent { get; set; }

        public string PointedPositionText { get; set; }

        public string PointedPositionBiomeName { get; set; }
    }
}