namespace AtomicTorch.CBND.CoreMod.Systems.Party
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public static class PartyConstants
    {
        public const double PartyInvitationLifetimeSeconds = 3 * 60; // 3 minutes

        public static ushort PartyMembersMax
            => RatePartyMembersMax.GetSharedValue(logErrorIfClientHasNoValue: false);
    }
}