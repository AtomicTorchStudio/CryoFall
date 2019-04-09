namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IProtoObjectFarm : IProtoObjectStructure
    {
        bool IsDrawingPlantShadow { get; }

        Vector2D PlacedPlantPositionOffset { get; }
    }
}