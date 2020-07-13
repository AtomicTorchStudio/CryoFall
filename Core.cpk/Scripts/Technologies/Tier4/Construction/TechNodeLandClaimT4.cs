namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;

    public class TechNodeLandClaimT4 : TechNode<TechGroupConstructionT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLandClaimT4>();
        }
    }
}