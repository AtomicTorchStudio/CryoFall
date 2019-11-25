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

        public ViewModelWindowObjectVehicle ViewModel { get; private set; }

        public static void CloseActiveMenu()
        {
            instance?.CloseWindow();
        }

        public static WindowObjectVehicle Open(
            IDynamicWorldObject objectVehicle,
            FrameworkElement vehicleExtraControl = null)
        {
            if (instance != null
                && instance.objectVehicle == objectVehicle)
            {
                return instance;
            }

            var window = new WindowObjectVehicle();
            instance = window;
            window.objectVehicle = objectVehicle;
            window.vehicleExtraControl = vehicleExtraControl;
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
            base.OnLoaded();

            this.DataContext = this.ViewModel =
                                   new ViewModelWindowObjectVehicle(this.objectVehicle, this.vehicleExtraControl);
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            this.DataContext = null;
            this.ViewModel.Dispose();
            this.ViewModel = null;
            instance = null;
        }
    }
}