namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;

    public class TechNodeCistern : TechNode<TechGroupConstructionT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectCistern>();

            config.SetRequiredNode<TechNodeLandClaimT4>();
        }
    }
}