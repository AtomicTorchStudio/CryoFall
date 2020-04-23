namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationVaseOrange : TechNode<TechGroupDecorations2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationVaseOrange>();

            config.SetRequiredNode<TechNodeDecorationAltar>();
        }
    }
}