namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Quests
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Quests.Data;

    public partial class WindowQuests : BaseWindowMenu
    {
        private ViewModelWindowQuests viewModel;

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = this.viewModel = new ViewModelWindowQuests();
        }

        protected override void WindowClosing()
        {
            base.WindowClosing();
            this.viewModel.RemoveNewFlagFromTheExpandedQuests();
        }

        protected override void WindowOpening()
        {
            base.WindowOpening();
            this.viewModel.RemoveNewFlagFromTheExpandedQuests();
            this.viewModel.IsCurrentQuestsTabSelected = true;
        }
    }
}