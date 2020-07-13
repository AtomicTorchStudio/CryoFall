namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Farming
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;

    public class TechNodePlantPot : TechNode<TechGroupFarmingT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectPlantPot>();

            config.SetRequiredNode<TechNodeSpices>();
        }
    }
}