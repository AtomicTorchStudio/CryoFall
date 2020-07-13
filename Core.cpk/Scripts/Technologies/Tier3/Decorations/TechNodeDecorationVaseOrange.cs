namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationVaseOrange : TechNode<TechGroupDecorationsT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationVaseOrange>();

            config.SetRequiredNode<TechNodeDecorationAltar>();
        }
    }
}