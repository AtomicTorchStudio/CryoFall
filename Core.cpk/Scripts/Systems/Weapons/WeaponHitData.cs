namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public readonly struct WeaponHitData : IRemoteCallParameter
    {
        public readonly IProtoWorldObject FallbackProtoWorldObject;

        public readonly Vector2Ushort FallbackTilePosition;

        public readonly IWorldObject WorldObject;

        public WeaponHitData(IWorldObject worldObject)
        {
            this.WorldObject = worldObject;
            this.FallbackProtoWorldObject = worldObject.ProtoWorldObject;
            this.FallbackTilePosition = worldObject.TilePosition;
        }
    }
}