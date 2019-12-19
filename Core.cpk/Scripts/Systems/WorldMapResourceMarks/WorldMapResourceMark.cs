namespace AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks
{
    using System;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    [NotPersistent]
    public readonly struct WorldMapResourceMark
        : IRemoteCallParameter,
          IEquatable<WorldMapResourceMark>
    {
        public WorldMapResourceMark(
            uint id,
            Vector2Ushort position,
            IProtoStaticWorldObject protoWorldObject,
            double serverSpawnTime,
            IProtoTile biome,
            Vector2Ushort searchAreaCirclePosition,
            ushort searchAreaCircleRadius)
        {
            this.Id = id;
            this.Position = position;
            this.ProtoWorldObject = protoWorldObject;
            this.ServerSpawnTime = serverSpawnTime;
            this.Biome = biome;
            this.SearchAreaCirclePosition = searchAreaCirclePosition;
            this.SearchAreaCircleRadius = searchAreaCircleRadius;
        }

        public IProtoTile Biome { get; }

        public uint Id { get; }

        /// <summary>
        /// Position of the world object center tile in the world.
        /// </summary>
        public Vector2Ushort Position { get; }

        public Vector2Ushort SearchAreaCirclePosition { get; }

        public ushort SearchAreaCircleRadius { get; }

        public IProtoStaticWorldObject ProtoWorldObject { get; }

        [TempOnly]
        public double ServerSpawnTime { get; }

        public bool Equals(WorldMapResourceMark other)
        {
            return this.Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is WorldMapResourceMark other
                   && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }
    }
}