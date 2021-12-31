namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Jewelry
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations;

    public class TechNodeDecorationGoldPileMedium : TechNode<TechGroupJewelryT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDecorationGoldPileMedium>();

            config.SetRequiredNode<TechNodeGoldArmor>();
        }
    }
}