namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeFurnaceElectric : TechNode<TechGroupElectricityT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFurnaceElectric>();

            config.SetRequiredNode<TechNodeGeneratorEngine>();
        }
    }
}