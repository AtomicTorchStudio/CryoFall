namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist
{
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowCompletionist : BaseWindowMenu
    {
        private TabControl tabControl;

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = ViewModelWindowCompletionist.Instance;
            this.tabControl = this.GetByName<TabControl>("TabControl");
        }

        protected override void OnLoaded()
        {
            this.tabControl.SelectionChanged += this.TabControlSelectionChangedHandler;
        }

        protected override void OnUnloaded()
        {
            this.tabControl.SelectionChanged -= this.TabControlSelectionChangedHandler;
        }

        protected override void WindowOpening()
        {
            base.WindowOpening();
            ViewModelWindowCompletionist.Instance.RefreshLists();
            this.ResetCurrentTabScroll();
        }

        private void ResetCurrentTabScroll()
        {
            foreach (var scrollViewer in VisualTreeHelperExtension.EnumerateAllChildsOfType<ScrollViewer>(
                (Visual)this.tabControl.SelectedContent))
            {
                scrollViewer.ScrollToTop();
                break;
            }
        }

        private void TabControlSelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            this.ResetCurrentTabScroll();
        }
    }
}