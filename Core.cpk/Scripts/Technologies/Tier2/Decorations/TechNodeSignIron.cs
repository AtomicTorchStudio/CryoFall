namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs;

    public class TechNodeSignIron : TechNode<TechGroupDecorationsT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectSignIron>();

            config.SetRequiredNode<TechNodeSignWood>();
        }
    }
}