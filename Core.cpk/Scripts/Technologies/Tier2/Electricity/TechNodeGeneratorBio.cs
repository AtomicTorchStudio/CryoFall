namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;

    public class TechNodeGeneratorBio : TechNode<TechGroupElectricityT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectGeneratorBio>();

            config.SetRequiredNode<TechNodeGeneratorSteam>();
        }
    }
}