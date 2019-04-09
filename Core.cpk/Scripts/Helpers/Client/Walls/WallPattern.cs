namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Walls
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class WallPattern
    {
        public readonly Vector2Ushort AtlasChunkPosition;

        public readonly double DrawOffsetDestroyed;

        public readonly double DrawOffsetNormal;

        public readonly string Name;

        public readonly NeighborsPattern RequiresNeighbors;

        public readonly Action<IPhysicsBody> SetupPhysicsDestroyed;

        public readonly Action<IPhysicsBody> SetupPhysicsNormal;

        public WallPattern(
            string name,
            Vector2Ushort atlasChunk,
            NeighborsPattern requiresNeighbors,
            double drawOffsetNormal = 0,
            double? drawOffsetDestroyed = null,
            Action<IPhysicsBody> physicsNormal = null,
            Action<IPhysicsBody> physicsDestroyed = null)
        {
            this.Name = name;
            this.AtlasChunkPosition = atlasChunk;
            this.RequiresNeighbors = requiresNeighbors;
            this.DrawOffsetNormal = drawOffsetNormal;
            this.DrawOffsetDestroyed = drawOffsetDestroyed ?? drawOffsetNormal;
            this.SetupPhysicsNormal = physicsNormal;
            this.SetupPhysicsDestroyed = physicsDestroyed ?? physicsNormal;
        }

        public bool IsPass(NeighborsPattern variant)
        {
            return (this.RequiresNeighbors & variant) == this.RequiresNeighbors;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}: {3}, {4}: {5}",
                                 nameof(this.Name),
                                 this.Name,
                                 nameof(this.RequiresNeighbors),
                                 this.RequiresNeighbors,
                                 nameof(this.AtlasChunkPosition),
                                 this.AtlasChunkPosition);
        }
    }
}