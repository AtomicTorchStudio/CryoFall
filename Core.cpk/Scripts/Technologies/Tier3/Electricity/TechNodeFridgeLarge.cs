namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;

    public class TechNodeFridgeLarge : TechNode<TechGroupElectricityT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFridgeLarge>();

            config.SetRequiredNode<TechNodeStoveElectric>();
        }
    }
}