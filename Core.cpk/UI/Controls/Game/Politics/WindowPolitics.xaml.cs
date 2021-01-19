namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics
{
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowPolitics : BaseWindowMenu
    {
        // start from the tab with index 1 as the tab with index 0 is a special placeholder
        private int lastSelectedIndex = 1;

        private TabControlCached tabControl;

        private ViewModelWindowPolitics viewModel;

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = this.viewModel = new ViewModelWindowPolitics();
            this.tabControl = this.GetByName<TabControlCached>("TabControl");
        }

        protected override void WindowClosing()
        {
            base.WindowClosing();
            this.lastSelectedIndex = this.tabControl.SelectedIndex;
            FactionSystem.ClientCurrentFactionChanged -= this.CurrentFactionChangedHandler;
            this.tabControl.SelectedItem = null;
        }

        protected override void WindowOpening()
        {
            base.WindowOpening();
            this.tabControl.SelectedIndex = this.lastSelectedIndex;
            FactionSystem.ClientCurrentFactionChanged += this.CurrentFactionChangedHandler;
            this.viewModel.Refresh();
        }

        private void CurrentFactionChangedHandler()
        {
            this.CloseWindow();

            if (FactionSystem.ClientHasFaction)
            {
                ClientTimersSystem.AddAction(0.1,
                                             Menu.Open<WindowFaction>);
            }
        }
    }
}