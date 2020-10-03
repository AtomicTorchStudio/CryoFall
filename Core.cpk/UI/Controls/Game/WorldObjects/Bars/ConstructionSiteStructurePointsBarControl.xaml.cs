namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ConstructionSiteStructurePointsBarControl : BaseUserControl, ICacheableControl
    {
        private ObjectStructurePointsData objectStructurePointsData;

        private ViewModelStructurePointsBarControl viewModel;

        public ObjectStructurePointsData ObjectStructurePointsData
        {
            get => this.objectStructurePointsData;
            set
            {
                if (this.objectStructurePointsData.Equals(value))
                {
                    return;
                }

                this.objectStructurePointsData = value;

                if (this.viewModel is not null)
                {
                    this.viewModel.ObjectStructurePointsData = this.objectStructurePointsData;
                }
            }
        }

        public void ResetControlForCache()
        {
            this.viewModel.ObjectStructurePointsData
                = this.objectStructurePointsData
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
            this.viewModel.ObjectStructurePointsData = this.objectStructurePointsData;
        }
    }
}