namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;

    public class TechNodeFridgeFreezer : TechNode<TechGroupElectricity3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFridgeFreezer>();

            config.SetRequiredNode<TechNodeWaterPump>();
        }
    }
}