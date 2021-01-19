namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TechNodeDecorationBannerBlack : TechNode<TechGroupDecorationsT2>
    {

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationBannerBlack>();

            config.SetRequiredNode<TechNodeDecorationBannerRed>();
        }
    }
}