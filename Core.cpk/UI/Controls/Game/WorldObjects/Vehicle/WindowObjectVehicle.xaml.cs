namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowObjectVehicle : BaseUserControlWithWindow
    {
        private static WindowObjectVehicle instance;

        private IDynamicWorldObject objectVehicle;

        private TabControl tabControl;

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
            if (instance != null
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

            this.tabControl = this.GetByName<WindowMenuWithInventory>("WindowMenuWithInventory")
                                  .GetByName<TabControl>("TabControl");
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.DataContext
                = this.ViewModel =
                      new ViewModelWindowObjectVehicle(this.objectVehicle,
                                                       this.vehicleExtraControl,
                                                       this.vehicleExtraControlViewModel,
                                                       this.ActiveTabChangedHandler);
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            this.DataContext = null;
            this.ViewModel.Dispose();
            this.ViewModel = null;

            if (instance == this)
            {
                instance = null;
            }
        }

        private void ActiveTabChangedHandler()
        {
            if (this.ViewModel is null)
            {
                return;
            }

            // NoesisGUI bug workaround to ensure the previously selected tab is clickable 
            // https://www.noesisengine.com/bugs/view.php?id=1751
            this.tabControl.Visibility = Visibility.Collapsed;
            this.tabControl.UpdateLayout();
            this.tabControl.Visibility = Visibility.Visible;
        }
    }
}