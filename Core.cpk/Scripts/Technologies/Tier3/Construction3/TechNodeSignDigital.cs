namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs;

    public class TechNodeSignDigital : TechNode<TechGroupConstruction3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectSignDigital>();

            config.SetRequiredNode<TechNodeFloorLampLarge>();
        }
    }
}