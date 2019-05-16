namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.LandClaims.Data
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowLandClaim : BaseViewModel
    {
        public const string AccessListEmpty = "You can provide other survivors with access to your land claim.";

        public const string AccessListTitle = "Access";

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

            this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(
                    landClaimWorldObject.GetPrivateState<ObjectLandClaimPrivateState>().ItemsContainer,
                    callbackTakeAllItemsSuccess: () => { })
                {
                    IsContainerTitleVisible = false
                };

            this.ViewModelProtoLandClaimInfoCurrent = new ViewModelProtoLandClaimInfo(this.protoObjectLandClaim);
        }

        public BaseCommand CommandUpgrade
            => new ActionCommand(this.ExecuteCommandUpgrade);

        public string FounderName => this.privateState.LandClaimFounder;

        public bool IsOwner => this.FounderName == ClientCurrentCharacterHelper.Character.Name;

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }

        public ViewModelWorldObjectOwnersEditor ViewModelOwnersEditor { get; }

        public ViewModelProtoLandClaimInfo ViewModelProtoLandClaimInfoCurrent { get; }

        public ViewModelProtoLandClaimInfo ViewModelProtoLandClaimInfoUpgrade { get; }

        public ViewModelStructureUpgrade ViewModelStructureUpgrade { get; }

        private void ExecuteCommandUpgrade()
        {
            var upgradeStructure = this.ViewModelStructureUpgrade.ViewModelUpgradedStructure.ProtoStructure;

            // please note: it will perform all the checks and display the error dialog if required
            this.protoObjectLandClaim.ClientUpgrade(
                this.landClaimWorldObject,
                (IProtoObjectLandClaim)upgradeStructure);
        }
    }
}