namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowDoor : BaseViewModel
    {
        private readonly ObjectDoorPrivateState privateState;

        public ViewModelWindowDoor(IStaticWorldObject worldObjectDoor)
        {
            var protoObjectDoor = (IProtoObjectDoor)worldObjectDoor.ProtoStaticWorldObject;
            this.privateState = protoObjectDoor.GetPrivateState(worldObjectDoor);

            this.IsInsideFactionClaim = LandClaimSystem.SharedIsWorldObjectOwnedByFaction(worldObjectDoor);
            if (this.IsInsideFactionClaim)
            {
                this.ViewModelFactionAccessEditor = new ViewModelWorldObjectFactionAccessEditorControl(
                    worldObjectDoor);
            }
            else
            {
                this.ViewModelOwnersEditor = new ViewModelWorldObjectOwnersEditor(
                    this.privateState.Owners,
                    callbackServerSetOwnersList:
                    ownersList => WorldObjectOwnersSystem.ClientSetOwners(worldObjectDoor,
                                                                          ownersList),
                    title: CoreStrings.ObjectOwnersList_Title2,
                    maxOwnersListLength: RateDoorOwnersMax.SharedValue);

                this.ViewModelDirectAccessEditor = new ViewModelWorldObjectDirectAccessEditor(
                    worldObjectDoor,
                    canSetAccessMode: true);
            }

            this.privateState.ClientSubscribe(_ => _.IsBlockedByShield,
                                              _ => this.NotifyPropertyChanged(nameof(this.IsBlockedByShield)),
                                              this);
        }

        public bool IsBlockedByShield => this.privateState.IsBlockedByShield;

        public bool IsInsideFactionClaim { get; set; }

        public ViewModelWorldObjectDirectAccessEditor ViewModelDirectAccessEditor { get; }

        public ViewModelWorldObjectFactionAccessEditorControl ViewModelFactionAccessEditor { get; }

        public ViewModelWorldObjectOwnersEditor ViewModelOwnersEditor { get; }
    }
}