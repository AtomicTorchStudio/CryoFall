namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;

    public class TechNodeWoodBarrel : TechNode<TechGroupConstructionT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectBarrelWood>();

            config.SetRequiredNode<TechNodeIronCrate>();
        }
    }
}