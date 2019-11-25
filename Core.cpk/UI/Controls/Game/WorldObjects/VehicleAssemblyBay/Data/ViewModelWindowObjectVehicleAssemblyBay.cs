namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.VehicleAssemblyBay
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowObjectVehicleAssemblyBay : BaseViewModel
    {
        private ViewModelVehicleSchematic selectedVehicleSchematic;

        public ViewModelWindowObjectVehicleAssemblyBay(
            IStaticWorldObject vehicleAssemblyBay,
            IReadOnlyCollection<IProtoVehicle> availableVehicleSchematics,
            int vehicleSchematicsCountTotal)
        {
            this.VehicleSchematicsList = availableVehicleSchematics.Select(v => new ViewModelVehicleSchematic(v))
                                                                   .ToList();
            this.VehicleSchematicsCountTotal = vehicleSchematicsCountTotal;

            this.SelectedVehicleSchematic = this.VehicleSchematicsList.FirstOrDefault();

            var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            this.ContainerInventory = (IClientItemsContainer)currentCharacter.SharedGetPlayerContainerInventory();

            this.ViewModelVehicleGarage = new ViewModelVehicleGarage(vehicleAssemblyBay);
        }

        public BaseCommand CommandBuild => new ActionCommand(this.ExecuteCommandBuild);

        public IClientItemsContainer ContainerInventory { get; }

        public ViewModelVehicleSchematic SelectedVehicleSchematic
        {
            get => this.selectedVehicleSchematic;
            set
            {
                if (this.selectedVehicleSchematic == value)
                {
                    return;
                }

                if (this.selectedVehicleSchematic != null)
                {
                    this.selectedVehicleSchematic.IsSelected = false;
                }

                this.selectedVehicleSchematic = value;

                if (this.selectedVehicleSchematic != null)
                {
                    this.selectedVehicleSchematic.IsSelected = true;
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public int VehicleSchematicsCountTotal { get; }

        public int VehicleSchematicsCountUnlocked => this.VehicleSchematicsList.Count;

        public IReadOnlyList<ViewModelVehicleSchematic> VehicleSchematicsList { get; }

        public ViewModelVehicleGarage ViewModelVehicleGarage { get; }

        private void ExecuteCommandBuild()
        {
            this.SelectedVehicleSchematic?.ProtoVehicle.ClientRequestBuild();
        }
    }
}