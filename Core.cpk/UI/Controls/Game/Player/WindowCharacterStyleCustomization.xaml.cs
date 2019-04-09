namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public partial class WindowCharacterStyleCustomization : BaseUserControlWithWindow
    {
        private ViewModelWindowCharacterStyleCustomization viewModel;

        protected override void InitControlWithWindow()
        {
            base.InitControlWithWindow();
            this.Window.IsCached = false;
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelWindowCharacterStyleCustomization(this.Window);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
        }
    }
}