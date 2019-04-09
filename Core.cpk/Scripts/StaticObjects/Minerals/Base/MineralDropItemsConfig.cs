namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class MineralDropItemsConfig
    {
        public readonly DropItemsList Stage1 = new DropItemsList();

        public readonly DropItemsList Stage2 = new DropItemsList();

        public readonly DropItemsList Stage3 = new DropItemsList();

        public readonly DropItemsList Stage4 = new DropItemsList();

        public ReadOnlyMineralDropItemsConfig AsReadOnly()
        {
            return new ReadOnlyMineralDropItemsConfig(this);
        }
    }
}