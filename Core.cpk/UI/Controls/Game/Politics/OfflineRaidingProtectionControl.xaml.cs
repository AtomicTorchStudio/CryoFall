namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class OfflineRaidingProtectionControl : BaseUserControl
    {
        public void Refresh()
        {
            var viewModel = (ViewModelOfflineRaidingProtectionControl)this.DataContext;
            viewModel.UpdateNextRaidingInfo();
        }

        protected override void InitControl()
        {
            this.DataContext = new ViewModelOfflineRaidingProtectionControl();
        }
    }
}