namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;

    public class TechNodeFridgeLarge : TechNode<TechGroupElectricity2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFridgeLarge>();

            config.SetRequiredNode<TechNodeStoveElectric>();
        }
    }
}