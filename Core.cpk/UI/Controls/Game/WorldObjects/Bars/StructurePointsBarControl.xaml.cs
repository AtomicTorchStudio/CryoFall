namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class StructurePointsBarControl : BaseUserControl, ICacheableControl
    {
        private float initialStructurePoints;

        private StaticObjectStructurePointsData staticObjectStructurePointsData;

        private ViewModelStructurePointsBarControl viewModel;

        public void ResetControlForCache()
        {
            this.viewModel.StaticObjectStructurePointsData
                = this.staticObjectStructurePointsData
                      = default;
        }

        public void Setup(
            StaticObjectStructurePointsData data,
            float initialStructurePoints)
        {
            this.initialStructurePoints = initialStructurePoints;
            if (this.staticObjectStructurePointsData.Equals(data))
            {
                return;
            }

            this.staticObjectStructurePointsData = data;
            if (this.viewModel != null)
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
            this.viewModel.StaticObjectStructurePointsData = this.staticObjectStructurePointsData;
            this.viewModel.SetInitialStructurePoints(this.initialStructurePoints);
        }
    }
}