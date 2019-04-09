namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;

    public class TechNodeWorkbench : TechNode<TechGroupIndustry>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectWorkbench>();
        }
    }
}