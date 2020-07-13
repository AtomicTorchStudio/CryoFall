namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodeSprinkler : TechNode<TechGroupElectricityT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectSprinkler>();

            config.SetRequiredNode<TechNodeWaterPump>();
        }
    }
}