namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodePsionicFieldGenerator : TechNode<TechGroupElectricity3>
    {
        public override FeatureAvailability AvailableIn => FeatureAvailability.OnlyPvP;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectPsionicFieldGenerator>();

            config.SetRequiredNode<TechNodeProjectorTower>();
        }
    }
}