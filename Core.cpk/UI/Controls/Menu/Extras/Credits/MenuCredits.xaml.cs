namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Extras.Credits
{
    using System.Windows.Controls;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuCredits : BaseUserControl
    {
        private AutoScrollViewer autoScrollViewer;

        protected override void InitControl()
        {
            var scrollViewer = this.GetByName<ScrollViewer>("ScrollViewer");
            this.autoScrollViewer = new AutoScrollViewer(scrollViewer);
        }
    }
}