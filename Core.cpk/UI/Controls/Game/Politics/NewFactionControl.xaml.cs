namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class NewFactionControl : BaseUserControl
    {
        private ViewModelNewFactionControl viewModel;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelNewFactionControl();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}