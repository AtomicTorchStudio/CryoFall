namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectFloor
        : IProtoObjectStructure, IProtoObjectMovementSurface, IProtoObjectWithGroundSoundMaterial
    {
        void ClientRefreshRenderer(IStaticWorldObject worldObject);
    }
}