namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeCompostTumbler : TechNode<TechGroupElectricityT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectCompostTumbler>();

            config.SetRequiredNode<TechNodePowerStorageLarge>();
        }
    }
}