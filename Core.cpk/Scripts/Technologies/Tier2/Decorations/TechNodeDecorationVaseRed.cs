namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationVaseRed : TechNode<TechGroupDecorations>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationVaseRed>();

            config.SetRequiredNode<TechNodeDecorationTable>();
        }
    }
}