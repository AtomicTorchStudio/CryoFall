namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Jewelry
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationStatue : TechNode<TechGroupJewelry>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationStatue>();

            config.SetRequiredNode<TechNodeGoldCrown>();
        }
    }
}