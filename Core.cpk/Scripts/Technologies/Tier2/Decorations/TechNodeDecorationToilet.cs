namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationToilet : TechNode<TechGroupDecorationsT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationToilet>();

            config.SetRequiredNode<TechNodeDecorationBookshelf>();
        }
    }
}