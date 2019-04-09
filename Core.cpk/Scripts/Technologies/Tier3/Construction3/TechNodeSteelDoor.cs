namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;

    public class TechNodeSteelDoor : TechNode<TechGroupConstruction3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectDoorSteel>();

            config.SetRequiredNode<TechNodeLandClaimT3>();
        }
    }
}