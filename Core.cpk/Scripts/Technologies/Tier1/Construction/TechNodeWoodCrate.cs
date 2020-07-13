namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;

    public class TechNodeWoodCrate : TechNode<TechGroupConstructionT1>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectCrateWood>();

            config.SetRequiredNode<TechNodeBasicBuilding>();
        }
    }
}