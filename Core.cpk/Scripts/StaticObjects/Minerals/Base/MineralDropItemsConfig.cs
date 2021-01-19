namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class MineralDropItemsConfig
    {
        public readonly DropItemsList Stage1 = new();

        public readonly DropItemsList Stage2 = new();

        public readonly DropItemsList Stage3 = new();

        public readonly DropItemsList Stage4 = new();

        public ReadOnlyMineralDropItemsConfig AsReadOnly()
        {
            return new(this);
        }
    }
}