namespace AtomicTorch.CBND.CoreMod.Systems.Party
{
    using System;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class PartyConstants
    {
        public const double PartyInvitationLifetimeSeconds = 3 * 60; // 3 minutes

        public static readonly double PartyLearningPointsSharePercent;

        public static Action ClientPartyMembersMaxChanged;

        static PartyConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            SharedPartyMembersMax
                = (ushort)MathHelper.Clamp(
                    ServerRates.Get(
                        "PartyMembersMax",
                        defaultValue: 5,
                        @"How many party members are allowed per party.
                          The value should be within 1-100 range."),
                    min: 1,
                    max: 100);

            PartyLearningPointsSharePercent
                = MathHelper.Clamp(
                      ServerRates.Get(
                          "PartyLearningPointsSharePercent",
                          defaultValue: 0,
                          @"This rate determines the percent of gained learning points 
                            distributed to other online party members.
                            By default, this feature is disabled: 0% of gained LP is deducted and shared among
                            all the other online party members equally.
                            You can enable it to any percent you want but generally we don't recommend going higher than 30!
                            If there are no other online party members, 100% of gained LP goes to the player who gained it.
                            You can set this value at any number from 0 to 100 percents if it makes any sense."),
                      min: 0.0,
                      max: 100.0)
                  / 100.0;

            PartyLearningPointsSharePercent = MathHelper.Clamp(PartyLearningPointsSharePercent, min: 0, max: 1);
        }

        public static ushort SharedPartyMembersMax { get; private set; }

        public static void ClientSetSystemConstants(ushort partyMembersMax)
        {
            Api.ValidateIsClient();

            SharedPartyMembersMax = partyMembersMax;
            Api.Logger.Info("Party member max size received from server: " + SharedPartyMembersMax);
            Api.SafeInvoke(ClientPartyMembersMaxChanged);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void EnsureInitialized()
        {
        }
    }
}