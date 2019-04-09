namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2
{
    using AtomicTorch.CBND.CoreMod.Perks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;

    public class TechNodeLandClaimT2 : TechNode<TechGroupConstruction2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLandClaimT2>()
                  .AddPerk<PerkIncreaseLandClaimLimitT2>();
        }
    }
}