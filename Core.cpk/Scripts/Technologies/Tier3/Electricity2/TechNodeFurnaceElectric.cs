namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage;

    public class TechNodeFurnaceElectric : TechNode<TechGroupElectricity2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFurnaceElectric>();

            config.SetRequiredNode<TechNodeGeneratorEngine>();
        }
    }
}