namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectDoorPublicState : StaticObjectPublicState
    {
        [SyncToClient]
        public bool IsHorizontalDoor { get; set; }

        [SyncToClient]
        [TempOnly]
        public bool IsOpened { get; set; }
    }
}