namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;

    public class TechNodeSteelDoor : TechNode<TechGroupConstructionT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDoorSteel>();

            config.SetRequiredNode<TechNodeBrickConstructions>();
        }
    }
}