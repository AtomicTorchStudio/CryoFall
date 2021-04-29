namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;

    public class TechNodeGeneratorPragmiumReactor : TechNode<TechGroupElectricityT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectGeneratorPragmium>();
        }
    }
}