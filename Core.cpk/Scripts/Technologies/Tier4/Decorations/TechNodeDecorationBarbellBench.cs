namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationBarbellBench : TechNode<TechGroupDecorationsT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationBarbellBench>();

            config.SetRequiredNode<TechNodeDecorationHatHanger>();
        }
    }
}