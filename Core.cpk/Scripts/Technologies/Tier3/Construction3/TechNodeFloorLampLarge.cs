namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;

    public class TechNodeFloorLampLarge : TechNode<TechGroupConstruction3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLightFloorLampLarge>();

            config.SetRequiredNode<TechNodeBed>();
        }
    }
}