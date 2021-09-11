namespace AtomicTorch.CBND.CoreMod.Technologies.Tier6.Escape
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodeLaunchpadStage2 : TechNode<TechGroupEscapeT6>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.SetRequiredNode<TechNodeLaunchpadStage1>();
            config.Effects
                  .AddStructure<ObjectLaunchpadStage2>();
        }
    }
}