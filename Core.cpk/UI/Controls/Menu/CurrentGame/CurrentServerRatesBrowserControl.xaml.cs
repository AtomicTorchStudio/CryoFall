namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.CurrentGame
{
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.CurrentGame.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CurrentServerRatesBrowserControl : BaseUserControl
    {
        private Grid layoutRoot;

        private ViewModelCurrentServerRatesBrowserControl viewModel;

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.layoutRoot.DataContext = this.viewModel = new ViewModelCurrentServerRatesBrowserControl();
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}