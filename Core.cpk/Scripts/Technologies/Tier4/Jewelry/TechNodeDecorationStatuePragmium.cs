namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Jewelry
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationStatuePragmium : TechNode<TechGroupJewelry>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationStatuePragmium>();

            config.SetRequiredNode<TechNodeGoldCrown>();
        }
    }
}