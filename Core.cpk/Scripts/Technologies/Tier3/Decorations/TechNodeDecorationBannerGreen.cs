namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationBannerGreen : TechNode<TechGroupDecorationsT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationBannerGreen>();

            config.SetRequiredNode<TechNodeDecorationVaseWhite>();
        }
    }
}