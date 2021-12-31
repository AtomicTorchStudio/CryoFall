namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class SkinsMenu : BaseUserControl
    {
        private ViewModelSkinMenu viewModel;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelSkinMenu();
            this.IsVisibleChanged += this.OnIsVisibleChanged;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
            this.IsVisibleChanged -= this.OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsVisible)
            {
                return;
            }

            this.viewModel.Reset();
            this.viewModel.IsFeaturedTabSelected = true;
        }
    }
}