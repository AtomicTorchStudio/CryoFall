namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;

    public class TechNodeFloorLampSmall : TechNode<TechGroupConstruction2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLightFloorLampSmall>();

            config.SetRequiredNode<TechNodeIronSign>();
        }
    }
}