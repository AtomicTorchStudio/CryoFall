namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class VehicleArmorBarControl : BaseUserControl, ICacheableControl
    {
        private float initialStructurePoints;

        private ObjectStructurePointsData objectStructurePointsData;

        private ViewModelStructurePointsBarControl viewModel;

        public void ResetControlForCache()
        {
            this.viewModel.ObjectStructurePointsData
                = this.objectStructurePointsData
                      = default;
        }

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
            if (this.viewModel is not null)
            {
                this.RefreshViewModelData();
            }
        }

        protected override void InitControl()
        {
            this.viewModel = new ViewModelStructurePointsBarControl();
            this.DataContext = this.viewModel;
            this.RefreshViewModelData();
        }

        private void RefreshViewModelData()
        {
            this.viewModel.ObjectStructurePointsData = this.objectStructurePointsData;
            this.viewModel.SetInitialStructurePoints(this.initialStructurePoints);
        }
    }
}