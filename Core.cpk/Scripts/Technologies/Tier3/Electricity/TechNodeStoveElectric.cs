namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeStoveElectric : TechNode<TechGroupElectricityT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectStoveElectric>();

            config.SetRequiredNode<TechNodeRechargingStation>();
        }
    }
}