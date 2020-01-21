namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;

    public class TechNodeFloorLampLarge : TechNode<TechGroupElectricity2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLightFloorLampLarge>();

            config.SetRequiredNode<TechNodeStoveElectric>();
        }
    }
}