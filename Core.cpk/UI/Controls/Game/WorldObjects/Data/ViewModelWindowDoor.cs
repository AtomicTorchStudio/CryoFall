namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowDoor : BaseViewModel
    {
        private readonly ObjectDoorPrivateState privateState;

        public ViewModelWindowDoor(
            IStaticWorldObject worldObjectDoor)
        {
            var protoObjectDoor = (IProtoObjectDoor)worldObjectDoor.ProtoStaticWorldObject;
            this.privateState = protoObjectDoor.GetPrivateState(worldObjectDoor);

            this.ViewModelOwnersEditor = new ViewModelWorldObjectOwnersEditor(
                this.privateState.Owners,
                callbackServerSetOwnersList:
                ownersList => WorldObjectOwnersSystem.ClientSetOwners(worldObjectDoor,
                                                                      ownersList),
                title: CoreStrings.ObjectOwnersList_Title2,
                maxOwnersListLength: StructureConstants.SharedDoorOwnersMax);

            this.ViewModelAccessModeEditor = new ViewModelWorldObjectAccessModeEditor(
                worldObjectDoor,
                canSetAccessMode: true);

            this.privateState.ClientSubscribe(_ => _.IsBlockedByShield,
                                              _ => this.NotifyPropertyChanged(nameof(this.IsBlockedByShield)),
                                              this);
        }

        public bool IsBlockedByShield => this.privateState.IsBlockedByShield;

        public ViewModelWorldObjectAccessModeEditor ViewModelAccessModeEditor { get; }

        public ViewModelWorldObjectOwnersEditor ViewModelOwnersEditor { get; }
    }
}