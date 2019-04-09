namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.XenoGeology
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeOilRefinery : TechNode<TechGroupXenogeology>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectOilRefinery>();

            config.SetRequiredNode<TechNodeOilPump>();
        }
    }
}