namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;

    public class TechNodeDisplayCaseLarge : TechNode<TechGroupDecorationsT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDisplayCaseLarge>();

            config.SetRequiredNode<TechNodeDecorationVaseWhite>();
        }
    }
}