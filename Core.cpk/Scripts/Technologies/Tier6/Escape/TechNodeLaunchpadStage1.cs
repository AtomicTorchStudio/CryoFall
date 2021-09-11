namespace AtomicTorch.CBND.CoreMod.Technologies.Tier6.Escape
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodeLaunchpadStage1 : TechNode<TechGroupEscapeT6>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLaunchpadStage1>();
        }
    }
}