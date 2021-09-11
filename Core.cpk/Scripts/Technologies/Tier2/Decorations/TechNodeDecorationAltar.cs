namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationAltar : TechNode<TechGroupDecorationsT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationAltar>();

            config.SetRequiredNode<TechNodeDecorationBannerFaction>();
        }
    }
}