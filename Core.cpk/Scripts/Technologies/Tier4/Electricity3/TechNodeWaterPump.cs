namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeWaterPump : TechNode<TechGroupElectricity3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectWaterPump>();

            config.SetRequiredNode<TechNodeGeneratorSolar>();
        }
    }
}