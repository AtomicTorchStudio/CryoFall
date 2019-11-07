namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;

    public class TechNodeGeneratorSolar : TechNode<TechGroupElectricity3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectGeneratorSolar>();

            //config.SetRequiredNode<TechNodeMetalBarrel>();
        }
    }
}