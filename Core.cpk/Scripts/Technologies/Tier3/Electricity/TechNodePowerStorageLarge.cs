namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage;

    public class TechNodePowerStorageLarge : TechNode<TechGroupElectricityT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectPowerStorageLarge>();

            config.SetRequiredNode<TechNodeWireFromPlastic>();
        }
    }
}