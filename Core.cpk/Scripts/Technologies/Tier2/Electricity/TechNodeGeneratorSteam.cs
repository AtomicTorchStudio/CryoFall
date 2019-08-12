namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;

    public class TechNodeGeneratorSteam : TechNode<TechGroupElectricity>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectGeneratorSteam>();

           config.SetRequiredNode<TechNodeWireFromRubber>();
        }
    }
}