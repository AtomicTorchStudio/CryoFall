namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectDoor : IProtoObjectWithOwnersList, IProtoObjectWithAccessMode
    {
        /// <summary>
        /// If set to null the door orientation is selected automatically.
        /// </summary>
        bool? IsHorizontalDoorOnly { get; }

        void ClientRefreshRenderer(IStaticWorldObject worldObjectDoor);

        ObjectDoorPrivateState GetPrivateState(IStaticWorldObject worldObjectDoor);
    }
}