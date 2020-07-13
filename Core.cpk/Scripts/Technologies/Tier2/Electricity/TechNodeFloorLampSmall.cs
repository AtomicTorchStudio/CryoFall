namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;

    public class TechNodeFloorLampSmall : TechNode<TechGroupElectricityT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLightFloorLampSmall>();

            config.SetRequiredNode<TechNodePowerStorage>();
        }
    }
}