namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.VehicleAssemblyBay
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowObjectVehicleAssemblyBay : BaseUserControlWithWindow
    {
        private static WindowObjectVehicleAssemblyBay instance;

        private IStaticWorldObject vehicleAssemblyBay;

        public ViewModelWindowObjectVehicleAssemblyBay ViewModel { get; private set; }

        public static void CloseActiveMenu()
        {
            instance?.CloseWindow();
        }

        public static WindowObjectVehicleAssemblyBay Open(IStaticWorldObject vehicleAssemblyBay)
        {
            if (instance != null
                && instance.vehicleAssemblyBay == vehicleAssemblyBay)
            {
                return instance;
            }

            var window = new WindowObjectVehicleAssemblyBay();
            instance = window;
            window.vehicleAssemblyBay = vehicleAssemblyBay;
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

            var protoVehicles = ProtoVehicleHelper.AllVehicles
                                                  .ToList();
            var recipesCountTotal = protoVehicles.Count;
            this.RemoveLockedVehicles(protoVehicles);

            this.DataContext = this.ViewModel = new ViewModelWindowObjectVehicleAssemblyBay(
                                   this.vehicleAssemblyBay,
                                   protoVehicles,
                                   recipesCountTotal);
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            this.DataContext = null;
            this.ViewModel.Dispose();
            this.ViewModel = null;
            instance = null;
        }

        private void RemoveLockedVehicles(List<IProtoVehicle> list)
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            list.RemoveAll(r => !r.SharedIsTechUnlocked(character));
        }
    }
}