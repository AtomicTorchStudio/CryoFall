namespace AtomicTorch.CBND.CoreMod.Events.Base
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EventWithAreaPublicState : EventPublicState
    {
        [SyncToClient]
        public Vector2Ushort AreaCirclePosition { get; set; }

        [SyncToClient]
        public ushort AreaCircleRadius { get; set; }

        public Vector2Ushort AreaEventOriginalPosition { get; set; }
    }
}