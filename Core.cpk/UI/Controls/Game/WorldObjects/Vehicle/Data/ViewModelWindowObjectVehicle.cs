namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data
{
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowObjectVehicle : BaseViewModel
    {
        private readonly VehiclePrivateState vehiclePrivateState;

        private readonly VehiclePublicState vehiclePublicState;

        private bool isPrimaryTabSelected;

        public ViewModelWindowObjectVehicle(IDynamicWorldObject vehicle, FrameworkElement vehicleExtraControl)
        {
            this.VehicleExtraControl = vehicleExtraControl;
            var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            this.ContainerPlayerInventory = (IClientItemsContainer)currentCharacter.SharedGetPlayerContainerInventory();

            this.ProtoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
            this.vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
            this.vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();

            var structurePointsMax = this.ProtoVehicle.SharedGetStructurePointsMax(vehicle);
            this.ViewModelStructurePoints = new ViewModelStructurePointsBarControl()
            {
                ObjectStructurePointsData = new ObjectStructurePointsData(vehicle, structurePointsMax)
            };

            this.ViewModelVehicleEnergy = new ViewModelVehicleEnergy(vehicle);

            this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(
                    this.vehiclePrivateState.CargoItemsContainer,
                    callbackTakeAllItemsSuccess: null)
                {
                    IsContainerTitleVisible = false
                };

            var isOwner = WorldObjectOwnersSystem.SharedIsOwner(
                ClientCurrentCharacterHelper.Character,
                vehicle);

            this.ViewModelOwnersEditor =
                new ViewModelWorldObjectOwnersEditor(this.vehiclePrivateState.Owners,
                                                     canEditOwners: isOwner
                                                                    || CreativeModeSystem.ClientIsInCreativeMode(),
                                                     callbackServerSetOwnersList:
                                                     ownersList => WorldObjectOwnersSystem.ClientSetOwners(
                                                         vehicle,
                                                         ownersList),
                                                     title: CoreStrings.ObjectOwnersList_Title + ":");

            this.RefreshCanRepair();

            this.IsPrimaryTabSelected = true;
        }

        public string CannotRepairErrorMessage { get; private set; }

        public BaseCommand CommandEnterVehicle => new ActionCommand(ExecuteCommandEnterVehicle);

        public BaseCommand CommandRepair => new ActionCommand(this.ExecuteCommandRepair);

        public IClientItemsContainer ContainerPlayerInventory { get; }

        public IClientItemsContainer FuelItemsContainer
            => (IClientItemsContainer)this.vehiclePrivateState.FuelItemsContainer;

        public bool HasCargoItemsContainer => this.vehiclePrivateState.CargoItemsContainer.SlotsCount > 0;

        public bool IsCanRepair { get; private set; }

        public bool IsPrimaryTabSelected
        {
            get => this.isPrimaryTabSelected;
            set
            {
                if (this.isPrimaryTabSelected == value)
                {
                    return;
                }

                this.isPrimaryTabSelected = value;
                this.NotifyThisPropertyChanged();

                if (this.isPrimaryTabSelected)
                {
                    var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
                    ClientContainersExchangeManager.Register(
                        this,
                        this.FuelItemsContainer,
                        allowedTargets: new[]
                        {
                            this.ContainerPlayerInventory,
                            currentCharacter.SharedGetPlayerContainerHotbar()
                        });
                }
                else
                {
                    ClientContainersExchangeManager.Unregister(this);
                }
            }
        }

        public IProtoVehicle ProtoVehicle { get; }

        public string RepairPercentPerStage => (100.0 / this.ProtoVehicle.RepairStagesCount).ToString("0.#");

        public uint RepairRequiredElectricityAmount => this.ProtoVehicle.RepairRequiredElectricityAmount;

        public IReadOnlyList<ProtoItemWithCount> RepairStageRequiredItems => this.ProtoVehicle.RepairStageRequiredItems;

        [ViewModelNotAutoDisposeField]
        public FrameworkElement VehicleExtraControl { get; }

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }

        public ViewModelWorldObjectOwnersEditor ViewModelOwnersEditor { get; }

        public ViewModelStructurePointsBarControl ViewModelStructurePoints { get; }

        public ViewModelVehicleEnergy ViewModelVehicleEnergy { get; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.IsPrimaryTabSelected = false;
        }

        private static void ExecuteCommandEnterVehicle()
        {
            VehicleSystem.ClientOnVehicleEnterOrExitRequest();
        }

        private void ExecuteCommandRepair()
        {
            this.ProtoVehicle.ClientRequestRepair();
        }

        private void RefreshCanRepair()
        {
            if (this.IsDisposed)
            {
                return;
            }

            ClientTimersSystem.AddAction(delaySeconds: 0.5, this.RefreshCanRepair);

            var checkResult = this.ProtoVehicle.SharedPlayerCanRepair(ClientCurrentCharacterHelper.Character);
            this.IsCanRepair = checkResult == VehicleCanRepairCheckResult.Success;

            switch (checkResult)
            {
                case VehicleCanRepairCheckResult.Success:
                    this.CannotRepairErrorMessage = null;
                    break;

                case VehicleCanRepairCheckResult.NotEnoughPower:
                    this.CannotRepairErrorMessage = PowerGridSystem.SetPowerModeResult.NotEnoughPower.GetDescription();
                    break;

                default:
                    this.CannotRepairErrorMessage = checkResult.GetDescription();
                    break;
            }
        }
    }
}