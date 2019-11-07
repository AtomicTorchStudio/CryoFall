namespace AtomicTorch.CBND.CoreMod.StaticObjects.Special.Base
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectCharredGroundPublicState : StaticObjectPublicState
    {
        [SyncToClient]
        public Vector2F WorldOffset { get; set; }
    }
}