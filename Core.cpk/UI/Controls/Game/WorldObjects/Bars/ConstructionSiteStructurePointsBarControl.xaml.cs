namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ConstructionSiteStructurePointsBarControl : BaseUserControl, ICacheableControl
    {
        private StaticObjectStructurePointsData staticObjectStructurePointsData;

        private ViewModelStructurePointsBarControl viewModel;

        public StaticObjectStructurePointsData StaticObjectStructurePointsData
        {
            get => this.staticObjectStructurePointsData;
            set
            {
                if (this.staticObjectStructurePointsData.Equals(value))
                {
                    return;
                }

                this.staticObjectStructurePointsData = value;

                if (this.viewModel != null)
                {
                    this.viewModel.StaticObjectStructurePointsData = this.staticObjectStructurePointsData;
                }
            }
        }

        public void ResetControlForCache()
        {
            this.viewModel.StaticObjectStructurePointsData
                = this.staticObjectStructurePointsData
                      = default;
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
        }
    }
}