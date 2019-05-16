namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Farming3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;

    public class TechNodePlantPot : TechNode<TechGroupFarming3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectPlantPot>();

            config.SetRequiredNode<TechNodeTobacco>();
        }
    }
}