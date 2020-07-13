namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodePsionicFieldGenerator : TechNode<TechGroupElectricityT5>
    {
        public override FeatureAvailability AvailableIn => FeatureAvailability.OnlyPvP;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectPsionicFieldGenerator>();

            config.SetRequiredNode<TechNodeRechargingStationAdvanced>();
        }
    }
}