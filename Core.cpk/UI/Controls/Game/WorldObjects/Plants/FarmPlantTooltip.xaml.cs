namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Plants
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Plants.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FarmPlantTooltip : BaseUserControl
    {
        private ViewModelFarmPlantTooltip viewModel;

        public IStaticWorldObject ObjectPlant { get; set; }

        public PlantPublicState PlantPublicState { get; set; }

        protected override void OnLoaded()
        {
            this.viewModel = new ViewModelFarmPlantTooltip(this.ObjectPlant, this.PlantPublicState);
            this.DataContext = this.viewModel;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }
    }
}