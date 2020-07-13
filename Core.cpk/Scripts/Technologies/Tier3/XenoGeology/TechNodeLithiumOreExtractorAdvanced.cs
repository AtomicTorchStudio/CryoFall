namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.XenoGeology
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeLithiumOreExtractorAdvanced : TechNode<TechGroupXenogeologyT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLithiumOreExtractorAdvanced>();

            config.SetRequiredNode<TechNodeLithiumOreExtractor>();
        }
    }
}