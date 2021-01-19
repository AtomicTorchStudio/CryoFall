namespace AtomicTorch.CBND.CoreMod.Editor.Data
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public readonly struct SpawnObjectRequest : IRemoteCallParameter
    {
        public readonly IProtoStaticWorldObject Prototype;

        public readonly Vector2Ushort TilePosition;

        public SpawnObjectRequest(IProtoStaticWorldObject prototype, Vector2Int tilePosition)
        {
            this.Prototype = prototype;
            this.TilePosition = tilePosition.ToVector2Ushort();
        }

        public SpawnObjectRequest(IStaticWorldObject staticWorldObject)
        {
            this.Prototype = staticWorldObject.ProtoStaticWorldObject;
            this.TilePosition = staticWorldObject.TilePosition;
        }
    }
}