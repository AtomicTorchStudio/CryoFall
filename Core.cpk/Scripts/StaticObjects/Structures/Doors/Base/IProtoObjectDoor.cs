namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectDoor : IProtoObjectWithOwnersList, IProtoObjectWithAccessMode
    {
        void ClientRefreshRenderer(IStaticWorldObject worldObjectDoor);

        ObjectDoorPrivateState GetPrivateState(IStaticWorldObject worldObjectDoor);
    }
}