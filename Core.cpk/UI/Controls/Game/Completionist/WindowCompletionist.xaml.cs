namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist.Data;

    public partial class WindowCompletionist : BaseWindowMenu
    {
        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = ViewModelWindowCompletionist.Instance;
        }

        protected override void WindowOpening()
        {
            base.WindowOpening();
            ViewModelWindowCompletionist.Instance.RefreshLists();
        }
    }
}