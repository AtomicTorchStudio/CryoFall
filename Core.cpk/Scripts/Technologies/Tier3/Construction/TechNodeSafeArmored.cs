namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;

    public class TechNodeSafeArmored : TechNode<TechGroupConstructionT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectSafeArmored>();

            config.SetRequiredNode<TechNodeMetalBarrel>();
        }
    }
}