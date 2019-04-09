namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;

    public class TechNodeWoodDoor : TechNode<TechGroupConstruction>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDoorWood>();

            config.SetRequiredNode<TechNodeBasicBuilding>();
        }
    }
}