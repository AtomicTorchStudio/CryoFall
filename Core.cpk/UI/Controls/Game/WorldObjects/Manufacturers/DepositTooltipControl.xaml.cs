namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class DepositTooltipControl : BaseUserControl
    {
        private IStaticWorldObject lastWorldObjectDeposit;

        private ViewModelDepositCapacityStatsControl viewModel;

        public void Setup(IStaticWorldObject worldObjectDeposit)
        {
            this.lastWorldObjectDeposit = worldObjectDeposit;
            this.RefreshViewModel();
        }

        protected override void OnLoaded()
        {
            this.RefreshViewModel();
        }

        protected override void OnUnloaded()
        {
            this.RefreshViewModel();
        }

        private void RefreshViewModel()
        {
            if (!this.isLoaded
                || this.lastWorldObjectDeposit == null)
            {
                if (this.viewModel == null)
                {
                    return;
                }

                this.DataContext = null;
                this.viewModel.Dispose();
                this.viewModel = null;
                return;
            }

            this.viewModel = new ViewModelDepositCapacityStatsControl(
                this.lastWorldObjectDeposit,
                tilePosition: this.lastWorldObjectDeposit.TilePosition);
            this.DataContext = this.viewModel;
        }
    }
}