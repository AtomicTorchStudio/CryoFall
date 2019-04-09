namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectMineral : IProtoStaticWorldObject
    {
        ReadOnlyMineralDropItemsConfig DropItemsConfig { get; }

        byte SharedCalculateDamageStage(float structurePoints);
    }
}