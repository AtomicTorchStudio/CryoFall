namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;

    public class TechNodeIronCrate : TechNode<TechGroupConstructionT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectCrateIron>();

            config.SetRequiredNode<TechNodeLandClaimT2>();
        }
    }
}