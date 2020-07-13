namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;

    public class TechNodeMetalBarrel : TechNode<TechGroupConstructionT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectBarrelMetal>();

            config.SetRequiredNode<TechNodeLargeSteelCrate>();
        }
    }
}