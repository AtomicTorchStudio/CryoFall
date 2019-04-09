namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs;

    public class TechNodeWoodSign : TechNode<TechGroupConstruction>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectSignWood>();

            config.SetRequiredNode<TechNodeLandClaimT1>();
        }
    }
}