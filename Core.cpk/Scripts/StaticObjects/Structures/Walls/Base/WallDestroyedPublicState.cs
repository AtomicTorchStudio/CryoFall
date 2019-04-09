namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class WallDestroyedPublicState : StaticObjectPublicState
    {
        [SyncToClient(isSendChanges: false)]
        public IProtoObjectWall OriginalProtoObjectWall { get; set; }
    }
}