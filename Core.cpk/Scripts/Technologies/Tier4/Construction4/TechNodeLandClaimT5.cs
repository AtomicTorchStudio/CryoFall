namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction4
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;

    public class TechNodeLandClaimT5 : TechNode<TechGroupConstruction4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLandClaimT5>();

            config.SetRequiredNode<TechNodeConcreteConstructions>();
        }
    }
}