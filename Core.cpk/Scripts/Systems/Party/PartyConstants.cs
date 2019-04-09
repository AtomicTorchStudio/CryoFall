namespace AtomicTorch.CBND.CoreMod.Systems.Party
{
    public static class PartyConstants
    {
        public const double PartyInvitationLifetimeSeconds = 3 * 60; // 3 minutes

        // Percent of gained LP distributed to other party members.
        public const double PartyLearningPointsSharePercent = 0.3; // 30%

        public const ushort PartyMembersMax = 10;
    }
}