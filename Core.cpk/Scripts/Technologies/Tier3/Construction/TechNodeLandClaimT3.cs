namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction
{
    using AtomicTorch.CBND.CoreMod.Perks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;

    public class TechNodeLandClaimT3 : TechNode<TechGroupConstructionT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLandClaimT3>()
                  .AddPerk<PerkIncreaseLandClaimLimitT3>();
        }
    }
}