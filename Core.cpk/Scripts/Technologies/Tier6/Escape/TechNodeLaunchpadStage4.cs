namespace AtomicTorch.CBND.CoreMod.Technologies.Tier6.Escape
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodeLaunchpadStage4 : TechNode<TechGroupEscapeT6>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.SetRequiredNode<TechNodeLaunchpadStage3>();
            config.Effects
                  .AddStructure<ObjectLaunchpadStage4>();
        }
    }
}