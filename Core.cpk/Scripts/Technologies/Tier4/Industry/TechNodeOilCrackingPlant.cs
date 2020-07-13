namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeOilCrackingPlant : TechNode<TechGroupIndustryT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectOilCrackingPlant>();

            config.SetRequiredNode<TechNodeComponentsHighTech>();
        }
    }
}