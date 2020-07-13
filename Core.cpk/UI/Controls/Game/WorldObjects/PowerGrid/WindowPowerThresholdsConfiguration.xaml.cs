namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public partial class WindowPowerThresholdsConfiguration : BaseUserControlWithWindow
    {
        private static WindowPowerThresholdsConfiguration Instance;

        private readonly IStaticWorldObject worldObject;

        private ViewModelWindowPowerThresholdsConfiguration viewModel;

        public WindowPowerThresholdsConfiguration(IStaticWorldObject worldObject)
        {
            this.worldObject = worldObject;
        }

        public WindowPowerThresholdsConfiguration()
        {
        }

        public ViewModelWindowPowerThresholdsConfiguration ViewModel => this.viewModel;

        public static void CloseWindowIfOpened()
        {
            Instance?.CloseWindow();
        }

        protected override void OnLoaded()
        {
            Instance = this;
            this.DataContext = this.viewModel =
                                   new ViewModelWindowPowerThresholdsConfiguration(
                                       this.worldObject,
                                       callbackSave: () => this.CloseWindow(DialogResult.OK),
                                       callbackCancel: () => this.CloseWindow(DialogResult.Cancel));
        }

        protected override void OnUnloaded()
        {
            if (ReferenceEquals(this, Instance))
            {
                Instance = null;
            }

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}