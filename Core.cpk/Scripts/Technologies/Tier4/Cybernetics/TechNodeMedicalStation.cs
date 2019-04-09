namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cybernetics
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodeMedicalStation : TechNode<TechGroupCybernetics>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectMedicalStation>();

            config.SetRequiredNode<TechNodeBiomaterialCollector>();
        }
    }
}