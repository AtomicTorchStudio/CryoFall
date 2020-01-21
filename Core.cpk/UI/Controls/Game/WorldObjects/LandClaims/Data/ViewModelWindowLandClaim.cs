namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.LandClaims.Data
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowLandClaim : BaseViewModel
    {
        public const string AccessListEmpty = "You can provide other survivors with access to your land claim.";

        public const string AccessListTitle = "Access";

        public const string DecayInfoDemoVersion =
            @"Please note: This land claim was [b]founded by a demo version player[/b].
              [br]Because of this, the delay before the decay starts as well as the decay duration itself are [b]shortened[/b].
              [br]When a demo player purchases the game, those durations revert to their standard values.";

        public const string DecayInfoFormat =
            @"Abandoned structures will start decaying after some time
              [br]and eventually disappear to prevent cluttering the map.
              [br]
              [br]This land claim building will start decaying in [b]{0}[/b].
              [br]After that it will decay in [b]{1}[/b] and then start the destruction timer ([b]{2}[/b]).
              [br]
              [br]In order to avoid the decay, simply visit this base at least once in [b]{0}[/b]
              [br](any player from the access list can trigger the reset of the decay timer).
              [br]
              [br]You can increase the decay delay by [b]upgrading[/b] any of the land claims to a higher tier.
              [br]The decay delay of the entire base is based on the [b]highest[/b] tier land claim building.";

        private readonly IStaticWorldObject landClaimWorldObject;

        private readonly LandClaimAreaPrivateState privateState;

        private readonly IProtoObjectLandClaim protoObjectLandClaim;

        public ViewModelWindowLandClaim(
            IStaticWorldObject landClaimWorldObject,
            ILogicObject area)
        {
            this.landClaimWorldObject = landClaimWorldObject;
            this.privateState = LandClaimArea.GetPrivateState(area);

            var protoLandClaim = (IProtoObjectLandClaim)landClaimWorldObject.ProtoStaticWorldObject;
            var canEditOwners = protoLandClaim
                .SharedCanEditOwners(landClaimWorldObject, ClientCurrentCharacterHelper.Character);

            this.ViewModelOwnersEditor = new ViewModelWorldObjectOwnersEditor(
                this.privateState.LandOwners,
                callbackServerSetOwnersList: ownersList => LandClaimSystem.ClientSetAreaOwners(
                                                 area,
                                                 ownersList),
                title: AccessListTitle + ":",
                emptyListMessage: AccessListEmpty,
                canEditOwners: canEditOwners,
                // exclude founder name
                ownersListFilter: name => name != this.FounderName);

            this.protoObjectLandClaim =
                (IProtoObjectLandClaim)this.landClaimWorldObject.ProtoStaticWorldObject;

            var upgrade = this.protoObjectLandClaim.ConfigUpgrade.Entries.FirstOrDefault();
            if (upgrade != null)
            {
                this.ViewModelStructureUpgrade = new ViewModelStructureUpgrade(upgrade);
                this.ViewModelProtoLandClaimInfoUpgrade = new ViewModelProtoLandClaimInfo(
                    (IProtoObjectLandClaim)upgrade.ProtoStructure);
            }

            var objectPublicState = landClaimWorldObject.GetPublicState<ObjectLandClaimPublicState>();
            objectPublicState.ClientSubscribe(
                _ => _.LandClaimAreaObject,
                _ => this.RefreshSafeStorageAndPowerGrid(),
                this);

            this.RefreshSafeStorageAndPowerGrid();

            this.ViewModelProtoLandClaimInfoCurrent = new ViewModelProtoLandClaimInfo(this.protoObjectLandClaim);

            ItemsContainerLandClaimSafeStorage.ClientSafeItemsSlotsCapacityChanged
                += this.SafeItemsSlotsCapacityChangedHandler;

            this.RequestDecayInfoTextAsync();
        }

        public BaseCommand CommandUpgrade
            => new ActionCommand(this.ExecuteCommandUpgrade);

        public string DecayInfoText { get; private set; } = CoreStrings.PleaseWait;

        public string FounderName => this.privateState.LandClaimFounder;

        public bool IsOwner => this.FounderName == ClientCurrentCharacterHelper.Character.Name
                               || CreativeModeSystem.ClientIsInCreativeMode();

        public bool IsSafeStorageAvailable => ItemsContainerLandClaimSafeStorage.ClientSafeItemsSlotsCapacity > 0;

        public bool IsSafeStorageCapacityExceeded => this.ViewModelItemsContainerExchange.Container.SlotsCount
                                                     > ItemsContainerLandClaimSafeStorage.ClientSafeItemsSlotsCapacity;

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; set; }

        public ViewModelWorldObjectOwnersEditor ViewModelOwnersEditor { get; }

        public ViewModelPowerGridState ViewModelPowerGridState { get; private set; }

        public ViewModelProtoLandClaimInfo ViewModelProtoLandClaimInfoCurrent { get; }

        public ViewModelProtoLandClaimInfo ViewModelProtoLandClaimInfoUpgrade { get; }

        public ViewModelStructureUpgrade ViewModelStructureUpgrade { get; }

        protected override void DisposeViewModel()
        {
            ItemsContainerLandClaimSafeStorage.ClientSafeItemsSlotsCapacityChanged
                -= this.SafeItemsSlotsCapacityChangedHandler;

            this.DisposeViewModelItemsContainerExchange();
            base.DisposeViewModel();
        }

        private void DisposeViewModelItemsContainerExchange()
        {
            var viewModel = this.ViewModelItemsContainerExchange;
            if (viewModel == null)
            {
                return;
            }

            this.ViewModelItemsContainerExchange = null;

            viewModel.Container.SlotsCountChanged
                -= this.SafeStorageSlotsChangedHandler;

            viewModel.Container.ItemsReset
                -= this.SafeStorageSlotsChangedHandler;

            viewModel.Dispose();
        }

        private void ExecuteCommandUpgrade()
        {
            var upgradeStructure = this.ViewModelStructureUpgrade.ViewModelUpgradedStructure.ProtoStructure;

            // please note: it will perform all the checks and display the error dialog if required
            this.protoObjectLandClaim.ClientUpgrade(
                this.landClaimWorldObject,
                (IProtoObjectLandClaim)upgradeStructure);
        }

        private void RefreshSafeStorageAndPowerGrid()
        {
            var objectPublicState = this.landClaimWorldObject.GetPublicState<ObjectLandClaimPublicState>();
            var area = objectPublicState.LandClaimAreaObject;
            var areasGroup = LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
            var areasGroupPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroup);

            // setup power grid
            var powerGrid = areasGroupPrivateState.PowerGrid;
            var oldViewModelPowerGridState = this.ViewModelPowerGridState;
            this.ViewModelPowerGridState = new ViewModelPowerGridState(PowerGrid.GetPublicState(powerGrid));
            oldViewModelPowerGridState?.Dispose();

            // setup safe storage
            this.DisposeViewModelItemsContainerExchange();

            this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(
                    areasGroupPrivateState.ItemsContainer,
                    callbackTakeAllItemsSuccess: () => { },
                    enableShortcuts: this.IsSafeStorageAvailable)
                {
                    IsContainerTitleVisible = false,
                };

            this.ViewModelItemsContainerExchange.Container.SlotsCountChanged
                += this.SafeStorageSlotsChangedHandler;

            this.ViewModelItemsContainerExchange.Container.ItemsReset
                += this.SafeStorageSlotsChangedHandler;
        }

        private async void RequestDecayInfoTextAsync()
        {
            var result = await LandClaimSystem.ClientGetDecayInfoText(this.landClaimWorldObject);
            if (this.IsDisposed)
            {
                return;
            }

            this.DecayInfoText = string.Format(
                DecayInfoFormat,
                ClientTimeFormatHelper.FormatTimeDuration(
                    TimeSpan.FromSeconds(result.DecayDelayDuration),
                    trimRemainder: true),
                ClientTimeFormatHelper.FormatTimeDuration(
                    TimeSpan.FromSeconds(result.DecayDuration),
                    trimRemainder: true),
                this.ViewModelProtoLandClaimInfoCurrent
                    .CurrentStructureLandClaimDestructionTimeout);
        }

        private void SafeItemsSlotsCapacityChangedHandler()
        {
            this.RefreshSafeStorageAndPowerGrid();
        }

        private void SafeStorageSlotsChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.IsSafeStorageCapacityExceeded));
        }
    }
}