namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationVaseOrange : TechNode<TechGroupDecorationsT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationVaseOrange>();

            config.SetRequiredNode<TechNodeDecorationBedsideTable>();
        }
    }
}