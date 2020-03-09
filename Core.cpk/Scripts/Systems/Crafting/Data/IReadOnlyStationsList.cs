namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IReadOnlyStationsList : IReadOnlyList<IProtoStaticWorldObject>
    {
        // Optimized to minimize GC allocations.
        bool Contains(IProtoWorldObject protoWorldObject);
    }
}