namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RatePvPIsFullLootEnabled
        : BaseRateBoolean<RatePvPIsFullLootEnabled>
    {
        [NotLocalizable]
        public override string Description =>
            @"For PvP servers you can enable full loot (dropping the equipped items on death)
              or keep it by default (not dropping the equipped items on death).
              Note: inventory items are dropped either way.";

        public override string Id => "PvP.IsFullLootEnabled";

        public override string Name => "[PvP] Drop full loot on death";

        public override bool ValueDefault => false;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}