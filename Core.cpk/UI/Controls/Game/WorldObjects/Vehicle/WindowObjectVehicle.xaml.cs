namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowObjectVehicle : BaseUserControlWithWindow
    {
        private static WindowObjectVehicle instance;

        private IDynamicWorldObject objectVehicle;

        private FrameworkElement vehicleExtraControl;

        private IViewModelWithActiveState vehicleExtraControlViewModel;

        public ViewModelWindowObjectVehicle ViewModel { get; private set; }

        public static void CloseActiveMenu()
        {
            instance?.CloseWindow();
        }

        public static WindowObjectVehicle Open(
            IDynamicWorldObject objectVehicle,
            FrameworkElement vehicleExtraControl = null,
            IViewModelWithActiveState vehicleExtraControlViewModel = null)
        {
            if (instance is not null
                && instance.objectVehicle == objectVehicle)
            {
                return instance;
            }

            var window = new WindowObjectVehicle();
            instance = window;
            window.objectVehicle = objectVehicle;
            window.vehicleExtraControl = vehicleExtraControl;
            window.vehicleExtraControlViewModel = vehicleExtraControlViewModel;
            Api.Client.UI.LayoutRootChildren.Add(window);
            return instance;
        }

        protected override void InitControlWithWindow()
        {
            // TODO: redone this to cached window when NoesisGUI implement proper Storyboard.Completed triggers
            this.Window.IsCached = false;
        }

        protected override void OnLoaded()
        {
            this.DataContext
                = this.ViewModel =
                      new ViewModelWindowObjectVehicle(this.objectVehicle,
                                                       this.vehicleExtraControl,
                                                       this.vehicleExtraControlViewModel);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.ViewModel.Dispose();
            this.ViewModel = null;

            if (instance == this)
            {
                instance = null;
            }
        }
    }
}