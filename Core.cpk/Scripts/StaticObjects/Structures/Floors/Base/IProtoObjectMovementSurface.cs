namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;

    [NotPersistent]
    [NotNetworkAvailable]
    public interface IProtoObjectMovementSurface : IProtoStaticWorldObject, IProtoObjectWithGroundSoundMaterial
    {
        /// <summary>
        /// Characters can move faster over this surface object.
        /// </summary>
        double CharacterMoveSpeedMultiplier { get; }
    }
}