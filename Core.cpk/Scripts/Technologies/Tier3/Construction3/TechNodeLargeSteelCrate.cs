namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;

    public class TechNodeLargeSteelCrate : TechNode<TechGroupConstruction3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectCrateSteelLarge>();

            config.SetRequiredNode<TechNodeSteelDoor>();
        }
    }
}