namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectFridge : IProtoObjectStructure
    {
        double FreshnessDurationMultiplier { get; }

        double ServerGetCurrentFreshnessDurationMultiplier(IStaticWorldObject worldObject);
    }
}