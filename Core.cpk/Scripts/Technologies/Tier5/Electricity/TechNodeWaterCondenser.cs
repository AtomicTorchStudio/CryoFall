namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeWaterCondenser : TechNode<TechGroupElectricityT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectWaterCondenser>();

            config.SetRequiredNode<TechNodeRechargingStationAdvanced>();
        }
    }
}