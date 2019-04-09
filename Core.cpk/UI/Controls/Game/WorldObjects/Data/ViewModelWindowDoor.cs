namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowDoor : BaseViewModel
    {
        public ViewModelWindowDoor(
            IStaticWorldObject worldObjectDoor)
        {
            var protoObjectDoor = (IProtoObjectDoor)worldObjectDoor.ProtoStaticWorldObject;
            var privateState = protoObjectDoor.GetPrivateState(worldObjectDoor);

            this.ViewModelOwnersEditor = new ViewModelWorldObjectOwnersEditor(
                privateState.Owners,
                callbackServerSetOwnersList:
                ownersList => WorldObjectOwnersSystem.ClientSetOwners(worldObjectDoor, ownersList),
                title: CoreStrings.ObjectOwnersList_Title + ":");

            this.ViewModelAccessModeEditor = new ViewModelWorldObjectAccessModeEditor(
                worldObjectDoor,
                canSetAccessMode: true);
        }

        public ViewModelWorldObjectAccessModeEditor ViewModelAccessModeEditor { get; }

        public ViewModelWorldObjectOwnersEditor ViewModelOwnersEditor { get; }
    }
}