namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;

    public class TechNodeIronDoor : TechNode<TechGroupConstruction2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDoorIron>();

            config.SetRequiredNode<TechNodeStoneConstructions>();
        }
    }
}