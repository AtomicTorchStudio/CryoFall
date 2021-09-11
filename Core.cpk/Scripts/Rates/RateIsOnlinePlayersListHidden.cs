namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateIsOnlinePlayersListHidden
        : BaseRateBoolean<RateIsOnlinePlayersListHidden>
    {
        [NotLocalizable]
        public override string Description =>
            @"You can hide the online players list.
              It's useful in PvP to prevent players from easily picking a target
              that is currently outnumbered or offline.
              Please note: online list is always visible for users with server operator or moderator access.";

        public override string Id => "IsOnlinePlayersListHidden";

        public override string Name => "Hide online players list";

        public override bool ValueDefault => false;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}