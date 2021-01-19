namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionApplicationsListControl : BaseUserControl
    {
        private ViewModelFactionApplicationsListControl viewModel;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelFactionApplicationsListControl();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}