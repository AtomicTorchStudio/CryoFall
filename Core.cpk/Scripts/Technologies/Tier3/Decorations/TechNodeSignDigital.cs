namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs;

    public class TechNodeSignDigital : TechNode<TechGroupDecorationsT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectSignDigital>();

            config.SetRequiredNode<TechNodeDisplayCaseLarge>();
        }
    }
}