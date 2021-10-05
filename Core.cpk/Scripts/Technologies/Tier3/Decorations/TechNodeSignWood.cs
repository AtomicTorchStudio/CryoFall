namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations
{
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs;

    public class TechNodeSignWood : TechNode<TechGroupDecorationsT3>
    {
        public override FeatureAvailability AvailableIn
            => IsServer && RateStructuresTextSignsAvailable.SharedValue
               || RateStructuresTextSignsAvailable.ClientIsValueReceived && RateStructuresTextSignsAvailable.SharedValue
                   ? FeatureAvailability.All
                   : FeatureAvailability.None;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectSignWood>();

            config.SetRequiredNode<TechNodeDecorationDoorBell2>();
        }
    }
}