namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public readonly struct DroppedLootInfo : IRemoteCallParameter
    {
        public DroppedLootInfo(Vector2Ushort position, double destroyAtTime)
        {
            this.Position = position;
            this.DestroyAtTime = destroyAtTime;
        }

        public double DestroyAtTime { get; }

        public Vector2Ushort Position { get; }
    }
}