namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class VehicleArmorBarControl : BaseUserControl
    {
        private float initialStructurePoints;

        private ObjectStructurePointsData objectStructurePointsData;

        private ViewModelStructurePointsBarControl viewModel;

        public void Setup(
            ObjectStructurePointsData data,
            float initialStructurePoints)
        {
            this.initialStructurePoints = initialStructurePoints;
            if (this.objectStructurePointsData.Equals(data))
            {
                return;
            }

            this.objectStructurePointsData = data;
            this.RefreshViewModelData();
        }

        protected override void OnLoaded()
        {
            this.viewModel = new ViewModelStructurePointsBarControl();
            this.RefreshViewModelData();
            this.DataContext = this.viewModel;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private void RefreshViewModelData()
        {
            if (this.viewModel is null)
            {
                return;
            }

            this.viewModel.ObjectStructurePointsData = this.objectStructurePointsData;
            this.viewModel.SetInitialStructurePoints(this.initialStructurePoints);
        }
    }
}