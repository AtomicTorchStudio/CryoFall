namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;

    public class TechNodeProjectorTower : TechNode<TechGroupElectricity3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLightProjectorTower>();

            config.SetRequiredNode<TechNodeGeneratorSolar>();
        }
    }
}