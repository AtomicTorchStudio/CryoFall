namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;

    public partial class WindowFaction : BaseWindowMenu
    {
        private ViewModelWindowFaction viewModel;

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = this.viewModel = new ViewModelWindowFaction();
        }
    }
}