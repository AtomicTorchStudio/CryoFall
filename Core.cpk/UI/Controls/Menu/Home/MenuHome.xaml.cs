namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Home
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Demo;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.FeaturesSlideshow;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Home.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuHome : BaseUserControl
    {
        private ViewModelMenuHome viewModel;

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = this.viewModel = new ViewModelMenuHome();

            DemoVersionWelcomeMenu.DisplayIfRequired();
            FeaturesSlideshow.DisplayIfRequired();

            Api.Client.MasterServer.DemoVersionInfoChanged += MasterServerDemoVersionInfoChangedHandler;
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            Api.Client.MasterServer.DemoVersionInfoChanged -= MasterServerDemoVersionInfoChangedHandler;
        }

        private static void MasterServerDemoVersionInfoChangedHandler()
        {
            DemoVersionWelcomeMenu.DisplayIfRequired();
        }
    }
}