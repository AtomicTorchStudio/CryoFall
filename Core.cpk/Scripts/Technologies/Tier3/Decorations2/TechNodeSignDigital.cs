namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs;

    public class TechNodeSignDigital : TechNode<TechGroupDecorations2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectSignDigital>();

            config.SetRequiredNode<TechNodeDecorationVaseWhite>();
        }
    }
}