namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationBannerBlue : TechNode<TechGroupDecorationsT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationBannerBlue>();

            config.SetRequiredNode<TechNodeDecorationBannerGreen>();
        }
    }
}