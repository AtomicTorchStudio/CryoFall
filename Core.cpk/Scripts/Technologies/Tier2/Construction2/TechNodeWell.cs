namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeWell : TechNode<TechGroupConstruction2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectWell>();

            config.SetRequiredNode<TechNodeIronCrate>();
        }
    }
}