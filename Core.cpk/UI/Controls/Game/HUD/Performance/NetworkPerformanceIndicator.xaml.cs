namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class NetworkPerformanceIndicator : BaseUserControl
    {
        private ViewModelNetworkPerformanceStats viewModel;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelNetworkPerformanceStats();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}