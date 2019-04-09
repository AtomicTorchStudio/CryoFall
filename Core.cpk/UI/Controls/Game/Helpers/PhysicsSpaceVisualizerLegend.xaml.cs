namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Helpers
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Helpers.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class PhysicsSpaceVisualizerLegend : BaseUserControl
    {
        private ViewModelPhysicsSpaceVisualizerLegend viewModel;

        public ViewModelPhysicsSpaceVisualizerLegend ViewModel => this.viewModel;

        public void Setup(ViewModelPhysicsSpaceVisualizerLegend viewModel)
        {
            this.viewModel = viewModel;
            this.Refresh();
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private void Refresh()
        {
            if (!this.isLoaded)
            {
                return;
            }

            this.DataContext = this.viewModel;
        }
    }
}