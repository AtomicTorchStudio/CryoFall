namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;

    public class TechNodeLandClaimT5 : TechNode<TechGroupConstructionT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLandClaimT5>();

            //config.SetRequiredNode<TechNodeConcreteConstructions>();
        }
    }
}