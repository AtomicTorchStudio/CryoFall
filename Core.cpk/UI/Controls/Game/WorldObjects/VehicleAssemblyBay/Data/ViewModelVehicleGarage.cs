namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.VehicleAssemblyBay
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelVehicleGarage : BaseViewModel
    {
        private readonly IStaticWorldObject vehicleAssemblyBay;

        private ViewModelGarageVehicleEntry selectedVehicle;

        public ViewModelVehicleGarage(IStaticWorldObject vehicleAssemblyBay)
        {
            this.vehicleAssemblyBay = vehicleAssemblyBay;
            this.Refresh();
        }

        public ObservableCollection<ViewModelGarageVehicleEntry> AccessibleVehicles { get; }
            = new SuperObservableCollection<ViewModelGarageVehicleEntry>();

        public bool CanPutCurrentVehicleToGarage { get; private set; }

        public bool CanTakeSelectedVehicle { get; private set; }

        public BaseCommand CommandPutCurrentVehicle
            => new ActionCommand(this.ExecuteCommandPutCurrentVehicle);

        public BaseCommand CommandTakeSelectedVehicle
            => new ActionCommand(this.ExecuteCommandTakeSelectedVehicle);

        // garage is always available in PvE
        // but in PvP it's available only if player has any vehicles in garage
        public bool IsGarageAvailable { get; private set; } =
            PveSystem.ClientIsPve(logErrorIfDataIsNotYetAvailable: false);

        public ViewModelGarageVehicleEntry SelectedVehicle
        {
            get => this.selectedVehicle;
            set
            {
                if (this.selectedVehicle == value)
                {
                    return;
                }

                this.selectedVehicle = value;
                this.NotifyThisPropertyChanged();

                this.RefreshCanTakeSelectedVehicle();
            }
        }

        private void ApplyCurrentVehicles(IReadOnlyList<GarageVehicleEntry> currentVehicles)
        {
            // remove all view models which are not existing in the current vehicles list
            for (var index = 0; index < this.AccessibleVehicles.Count; index++)
            {
                var viewModel = this.AccessibleVehicles[index];
                var isFoundInCurrentEntries = false;
                foreach (var entry in currentVehicles)
                {
                    if (viewModel.VehicleGameObjectId != entry.Id)
                    {
                        continue;
                    }

                    isFoundInCurrentEntries = true;
                    break;
                }

                if (isFoundInCurrentEntries)
                {
                    continue;
                }

                // this view model has no corresponding vehicle entry anymore
                this.AccessibleVehicles.RemoveAt(index--);
                viewModel.Dispose();
            }

            // update status or create view models for current vehicles
            foreach (var entry in currentVehicles)
            {
                var isFound = false;
                foreach (var viewModel in this.AccessibleVehicles)
                {
                    if (viewModel.VehicleGameObjectId != entry.Id)
                    {
                        continue;
                    }

                    isFound = true;
                    viewModel.Status = entry.Status;
                    break;
                }

                if (!isFound)
                {
                    this.AccessibleVehicles.Add(
                        new ViewModelGarageVehicleEntry(entry.Id, entry.ProtoVehicle, entry.Status));
                }
            }
        }

        private void ExecuteCommandPutCurrentVehicle()
        {
            VehicleGarageSystem.ClientPutCurrentVehicle();
        }

        private void ExecuteCommandTakeSelectedVehicle()
        {
            var entry = this.SelectedVehicle;
            if (entry is null)
            {
                return;
            }

            VehicleGarageSystem.ClientTakeVehicle(entry.VehicleGameObjectId);
        }

        private async void Refresh()
        {
            if (this.IsDisposed)
            {
                return;
            }

            ClientTimersSystem.AddAction(delaySeconds: 0.5,
                                         this.Refresh);

            var protoVab = (IProtoVehicleAssemblyBay)this.vehicleAssemblyBay.ProtoGameObject;
            using var tempVehiclesOnPlatform = Api.Shared.GetTempList<IDynamicWorldObject>();
            protoVab.SharedGetVehiclesOnPlatform(this.vehicleAssemblyBay, tempVehiclesOnPlatform);

            var isPvE = PveSystem.ClientIsPve(logErrorIfDataIsNotYetAvailable: false);
            this.CanPutCurrentVehicleToGarage = isPvE
                                                && tempVehiclesOnPlatform.Count > 0;

            this.RefreshCanTakeSelectedVehicle();

            var currentVehicles = await VehicleGarageSystem.ClientGetVehiclesListAsync();
            if (this.IsDisposed)
            {
                return;
            }

            this.ApplyCurrentVehicles(currentVehicles);
            this.IsGarageAvailable = isPvE
                                     || currentVehicles.Count > 0;
        }

        private void RefreshCanTakeSelectedVehicle()
        {
            var vehicleEntry = this.selectedVehicle;
            if (vehicleEntry is null)
            {
                this.CanTakeSelectedVehicle = false;
                return;
            }

            switch (vehicleEntry.Status)
            {
                case VehicleStatus.InGarage:
                case VehicleStatus.InWorld:
                    this.CanTakeSelectedVehicle = true;
                    break;

                case VehicleStatus.Docked:
                    // can take a docked vehicle only from another VAB
                    var vehicle = Client.World.GetGameObjectById<IDynamicWorldObject>(
                        GameObjectType.DynamicObject,
                        vehicleEntry.VehicleGameObjectId);

                    this.CanTakeSelectedVehicle = vehicle is null
                                                  || !vehicle.IsInitialized
                                                  || !VehicleGarageSystem.ClientIsVehicleDocked(
                                                      vehicle,
                                                      this.vehicleAssemblyBay);
                    break;

                default:
                    this.CanTakeSelectedVehicle = false;
                    break;
            }
        }
    }
}