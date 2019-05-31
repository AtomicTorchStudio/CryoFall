namespace AtomicTorch.CBND.CoreMod.Systems.Party
{
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class PartyConstants
    {
        public const double PartyInvitationLifetimeSeconds = 3 * 60; // 3 minutes

        public static readonly double PartyLearningPointsSharePercent;

        public static readonly ushort ServerPartyMembersMax;

        static PartyConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            ServerPartyMembersMax
                = (ushort)MathHelper.Clamp(
                    ServerRates.Get(
                        "PartyMembersMax",
                        defaultValue: 10,
                        @"How many party members are allowed per party.
                          The value should be within 1-100 range."),
                    min: 1,
                    max: 100);

            PartyLearningPointsSharePercent
                = MathHelper.Clamp(
                      ServerRates.Get(
                          "PartyLearningPointsSharePercent",
                          defaultValue: 30.0,
                          @"This rate determines the percent of gained learning points 
                            distributed to other online party members.
                            By default, 30% of gained LP is deducted and shared among
                            all the other online party members equally.
                            You also receive part of the said 30% LP from all other party members.
                            If there are no other online party members, 100% of gained LP goes to the player who gained it.
                            You can set this value at any number from 0 to 100 percents if it makes any sense."),
                      min: 0.0,
                      max: 100.0)
                  / 100.0;
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void EnsureInitialized()
        {
        }
    }
}