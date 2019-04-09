namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation
{
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;

    public interface IProtoObjectGatherableVegetation
        : IProtoObjectVegetation,
          IProtoObjectGatherable
    {
        IReadOnlyDropItemsList GatherDroplist { get; }
    }
}