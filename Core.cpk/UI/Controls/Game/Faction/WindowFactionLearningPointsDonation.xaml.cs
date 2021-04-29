namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowFactionLearningPointsDonation : BaseUserControlWithWindow
    {
        private ViewModelWindowFactionLearningPointsDonation viewModel;

        public static void Open()
        {
            var window = new WindowFactionLearningPointsDonation();
            Api.Client.UI.LayoutRootChildren.Add(window);
        }

        protected override void OnLoaded()
        {
            this.DataContext
                = this.viewModel
                      = new ViewModelWindowFactionLearningPointsDonation(
                          callbackCloseWindow: () => this.CloseWindow());
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}