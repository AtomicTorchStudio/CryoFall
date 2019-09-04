namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectFloor : IProtoObjectStructure, IProtoObjectWithGroundSoundMaterial
    {
        /// <summary>
        /// Characters can move faster on this floor.
        /// </summary>
        double CharacterMoveSpeedMultiplier { get; }

        void ClientRefreshRenderer(IStaticWorldObject worldObject);
    }
}