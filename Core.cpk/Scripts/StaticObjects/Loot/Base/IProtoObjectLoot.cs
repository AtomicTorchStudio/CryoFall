namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectLoot : IProtoStaticWorldObject
    {
        bool IsAvailableInCompletionist { get; }

        IReadOnlyDropItemsList LootDroplist { get; }
    }
}