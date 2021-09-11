namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs;

    public class TechNodeSignWood : TechNode<TechGroupDecorationsT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectSignWood>();

            config.SetRequiredNode<TechNodeDecorationDoorBell2>();
        }
    }
}