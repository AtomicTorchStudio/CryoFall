namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowObjectVehicle : BaseViewModel
    {
        private readonly IClientItemsContainer cargoItemsContainer;

        private readonly IDynamicWorldObject vehicle;

        private readonly VehiclePrivateState vehiclePrivateState;

        private readonly VehiclePublicState vehiclePublicState;

        private bool isVehicleTabActive;

        public ViewModelWindowObjectVehicle(
            IDynamicWorldObject vehicle,
            FrameworkElement vehicleExtraControl,
            IViewModelWithActiveState vehicleExtraControlViewModel)
        {
            this.vehicle = vehicle;
            this.VehicleExtraControl = vehicleExtraControl;
            this.VehicleExtraControlViewModel = vehicleExtraControlViewModel;
            if (vehicleExtraControl is not null)
            {
                vehicleExtraControl.DataContext = vehicleExtraControlViewModel;
            }

            var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            this.ContainerPlayerInventory = (IClientItemsContainer)currentCharacter.SharedGetPlayerContainerInventory();

            this.ProtoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
            this.vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
            this.vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();

            this.vehiclePublicState.ClientSubscribe(_ => _.ClanTag,
                                                    _ =>
                                                    {
                                                        this.NotifyPropertyChanged(nameof(this.IsOwnedByFaction));
                                                        this.NotifyPropertyChanged(nameof(this.FactionClanTag));
                                                        this.NotifyPropertyChanged(nameof(this.FactionEmblem));
                                                        this.RefreshAccessEditor();
                                                    },
                                                    this);

            var structurePointsMax = this.ProtoVehicle.SharedGetStructurePointsMax(vehicle);
            this.ViewModelStructurePoints = new ViewModelStructurePointsBarControl()
            {
                ObjectStructurePointsData = new ObjectStructurePointsData(vehicle, structurePointsMax)
            };

            this.ViewModelVehicleEnergy = new ViewModelVehicleEnergy(vehicle);

            this.cargoItemsContainer = this.vehiclePrivateState.CargoItemsContainer as IClientItemsContainer;
            this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(this.cargoItemsContainer)
            {
                IsContainerTitleVisible = false,
                IsActive = false
            };

            this.RefreshAccessEditor();
            this.RefreshCanRepair();

            this.IsVehicleTabActive = true;
            this.ViewModelItemsContainerExchange.IsActive = true;

            if (this.cargoItemsContainer is not null)
            {
                this.cargoItemsContainer.ItemAdded += this.CargoItemsContainerItemAddedHandler;
                this.cargoItemsContainer.ItemCountChanged += this.CargoItemsContainerItemCountChangedHandler;
            }
        }

        public string CannotRepairErrorMessage { get; private set; }

        public bool CanTransferToFactionOwnership
            => FactionSystem.ClientCurrentFaction is not null
               && FactionSystem.ClientCurrentFactionKind != FactionKind.Public;

        public BaseCommand CommandEnterVehicle => new ActionCommand(ExecuteCommandEnterVehicle);

        public BaseCommand CommandRepair => new ActionCommand(this.ExecuteCommandRepair);

        public BaseCommand CommandTransferToFactionOwnership =>
            new ActionCommand(this.ExecuteCommandTransferToFactionOwnership);

        public IClientItemsContainer ContainerPlayerInventory { get; }

        public string FactionClanTag => this.vehiclePublicState.ClanTag;

        public Brush FactionEmblem
            => ClientFactionEmblemCache.GetEmblemTextureBrush(this.vehiclePublicState.ClanTag);

        public string FactionPermissionName => FactionMemberAccessRights.LandClaimManagement.GetDescription();

        public IClientItemsContainer FuelItemsContainer
            => (IClientItemsContainer)this.vehiclePrivateState.FuelItemsContainer;

        public bool HasCargoItemsContainer => this.vehiclePrivateState.CargoItemsContainer.SlotsCount > 0;

        public bool IsCanRepair { get; private set; }

        public bool IsCargoTabActive { get; set; }

        public bool IsOwnedByFaction => !string.IsNullOrEmpty(this.FactionClanTag);

        public bool IsVehicleTabActive
        {
            get => this.isVehicleTabActive;
            set
            {
                if (this.isVehicleTabActive == value)
                {
                    return;
                }

                this.isVehicleTabActive = value;

                var wasContainerExchangeActive = this.ViewModelItemsContainerExchange.IsActive;
                this.ViewModelItemsContainerExchange.IsActive = false;

                if (this.isVehicleTabActive)
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

                if (this.VehicleExtraControlViewModel is not null)
                {
                    this.VehicleExtraControlViewModel.IsActive = value;
                }

                this.ViewModelItemsContainerExchange.IsActive = wasContainerExchangeActive;

                this.NotifyThisPropertyChanged();
            }
        }

        public IProtoVehicle ProtoVehicle { get; }

        public string RepairPercentPerStage => (100.0 / this.ProtoVehicle.RepairStagesCount).ToString("0.#");

        public uint RepairRequiredElectricityAmount => this.ProtoVehicle.RepairRequiredElectricityAmount;

        public IReadOnlyList<ProtoItemWithCount> RepairStageRequiredItems => this.ProtoVehicle.RepairStageRequiredItems;

        [ViewModelNotAutoDisposeField]
        public FrameworkElement VehicleExtraControl { get; }

        public ViewModelWorldObjectFactionAccessEditorControl ViewModelFactionAccessEditor { get; set; }

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }

        public ViewModelWorldObjectOwnersEditor ViewModelOwnersEditor { get; set; }

        public ViewModelStructurePointsBarControl ViewModelStructurePoints { get; }

        public ViewModelVehicleEnergy ViewModelVehicleEnergy { get; }

        private IViewModelWithActiveState VehicleExtraControlViewModel { get; }

        protected override void DisposeViewModel()
        {
            this.IsVehicleTabActive = false;

            if (this.cargoItemsContainer is not null)
            {
                this.cargoItemsContainer.ItemAdded -= this.CargoItemsContainerItemAddedHandler;
                this.cargoItemsContainer.ItemCountChanged -= this.CargoItemsContainerItemCountChangedHandler;
            }

            base.DisposeViewModel();
        }

        private static void ExecuteCommandEnterVehicle()
        {
            VehicleSystem.ClientOnVehicleEnterOrExitRequest();
        }

        private void CargoItemsContainerItemAddedHandler(IItem item)
        {
            this.IsCargoTabActive = true;
        }

        private void CargoItemsContainerItemCountChangedHandler(IItem item, ushort previousCount, ushort currentCount)
        {
            if (currentCount > previousCount)
            {
                this.IsCargoTabActive = true;
            }
        }

        private void ExecuteCommandRepair()
        {
            this.ProtoVehicle.ClientRequestRepair();
        }

        private void ExecuteCommandTransferToFactionOwnership()
        {
            VehicleSystem.ClientTransferToFactionOwnership(this.vehicle);
        }

        private void RefreshAccessEditor()
        {
            IDisposable vm = this.ViewModelFactionAccessEditor;
            this.ViewModelFactionAccessEditor = null;
            vm?.Dispose();

            vm = this.ViewModelOwnersEditor;
            this.ViewModelOwnersEditor = null;
            vm?.Dispose();

            if (this.IsOwnedByFaction)
            {
                this.ViewModelFactionAccessEditor = new ViewModelWorldObjectFactionAccessEditorControl(
                    this.vehicle,
                    canSetAccessMode: true);
            }
            else
            {
                var isOwner = WorldObjectOwnersSystem.SharedIsOwner(
                    ClientCurrentCharacterHelper.Character,
                    this.vehicle);

                this.ViewModelOwnersEditor =
                    new ViewModelWorldObjectOwnersEditor(
                        this.vehiclePrivateState.Owners,
                        canEditOwners: isOwner
                                       || CreativeModeSystem.ClientIsInCreativeMode(),
                        callbackServerSetOwnersList: ownersList => WorldObjectOwnersSystem.ClientSetOwners(
                                                         this.vehicle,
                                                         ownersList),
                        title: CoreStrings.ObjectOwnersList_Title2);
            }
        }

        private void RefreshCanRepair()
        {
            if (this.IsDisposed)
            {
                return;
            }

            ClientTimersSystem.AddAction(delaySeconds: 0.5, this.RefreshCanRepair);

            var checkResult = this.ProtoVehicle.SharedPlayerCanRepairInVehicleAssemblyBay(
                ClientCurrentCharacterHelper.Character);
            this.IsCanRepair = checkResult == VehicleCanRepairCheckResult.Success;

            this.CannotRepairErrorMessage = checkResult switch
            {
                VehicleCanRepairCheckResult.Success => null,
                VehicleCanRepairCheckResult.NotEnoughPower => PowerGridSystem.SetPowerModeResult.NotEnoughPower
                                                                             .GetDescription(),
                _ => checkResult.GetDescription()
            };
        }
    }
}