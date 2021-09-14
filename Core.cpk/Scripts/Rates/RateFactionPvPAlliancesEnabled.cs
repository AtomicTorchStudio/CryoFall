namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateFactionPvPAlliancesEnabled
        : BaseRateBoolean<RateFactionPvPAlliancesEnabled>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines whether the alliances list is available for PvP servers.
              By default PvP alliances are allowed.
              Change to 0 to disable alliances. Please note: already created alliance will remain.";

        public override string Id => "Faction.PvP.AlliancesEnabled";

        public override string Name => "[Faction] [PvP] Alliances permitted";

        public override bool ValueDefault => true;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}