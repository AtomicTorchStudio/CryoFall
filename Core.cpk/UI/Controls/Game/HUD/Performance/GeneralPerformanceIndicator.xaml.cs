namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class GeneralPerformanceIndicator : BaseUserControl
    {
        private ViewModelGeneralPerformanceStats viewModel;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelGeneralPerformanceStats();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}