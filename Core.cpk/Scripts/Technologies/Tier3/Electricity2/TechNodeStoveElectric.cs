namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeStoveElectric : TechNode<TechGroupElectricity2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectStoveElectric>();

            config.SetRequiredNode<TechNodeWireFromPlastic>();
        }
    }
}