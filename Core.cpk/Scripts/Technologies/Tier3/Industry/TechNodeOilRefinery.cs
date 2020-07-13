namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeOilRefinery : TechNode<TechGroupIndustryT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectOilRefinery>();

            config.SetRequiredNode<TechNodeCanisterEmpty>();
        }
    }
}