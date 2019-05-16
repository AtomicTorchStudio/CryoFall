namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDDuelModeControl : BaseUserControl
    {
        private ViewModelHUDDuelModeControl viewModel;

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            this.viewModel = new ViewModelHUDDuelModeControl();
            this.DataContext = this.viewModel;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}