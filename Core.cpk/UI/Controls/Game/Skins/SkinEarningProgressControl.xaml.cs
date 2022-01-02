namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class SkinEarningProgressControl : BaseUserControl
    {
        private ViewModelSkinEarningProgressControl viewModel;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelSkinEarningProgressControl();
            this.IsVisibleChanged += IsVisibleChangedHandler;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            this.IsVisibleChanged -= IsVisibleChangedHandler;
        }

        private static void IsVisibleChangedHandler(
            object sender,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            //((SkinEarningProgressControl)sender).viewModel?.ResetProgressBar();
        }
    }
}