namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class FactionConstants
    {
        public const double FactionApplicationLifetimeSeconds = 48 * 60 * 60; // 48 hours

        public const double FactionInvitationLifetimeSeconds = 48 * 60 * 60; // 48 hours

        public const byte MaxFactionLevel = 10;

        private static ushort sharedPrivateFactionMembersMax;

        private static ushort sharedPublicFactionMembersMax;

        static FactionConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            sharedPublicFactionMembersMax
                = (ushort)MathHelper.Clamp(
                    ServerRates.Get(
                        "Faction.MembersMax.PublicFaction",
                        defaultValue: 250,
                        @"How many faction members are allowed for a public faction
                          (that anyone can join freely at any time).
                          IMPORTANT: You can set this to 0 to disable public factions altogether.
                          The value should be within 0-250 range."),
                    min: 0,
                    max: 250);

            sharedPrivateFactionMembersMax
                = (ushort)MathHelper.Clamp(
                    ServerRates.Get(
                        "Faction.MembersMax.PrivateFaction",
                        defaultValue: 25,
                        @"How many faction members are allowed for a private faction
                          (that players can join only by submitting an application or receiving an invite).
                          The value should be within 1-250 range."),
                    min: 1,
                    max: 250);

            SharedCreateFactionLearningPointsCost
                = (ushort)MathHelper.Clamp(
                    ServerRates.Get(
                        "Faction.CreateCostLearningPoints",
                        defaultValue: PveSystem.ServerIsPvE ? 100 : 200,
                        @"How many learning points are required in order to create a faction.
                          The default value is 100 for PvE and 200 for PvP servers.
                          The value should be within 1-65000 range."),
                    min: 1,
                    max: 65000);

            SharedFactionJoinCooldownDuration
                = (uint)MathHelper.Clamp(
                    ServerRates.Get(
                        "Faction.JoinCooldownDuration",
                        defaultValue: 24 * 60 * 60,
                        @"Faction re-join/switch cooldown duration (in seconds).
                          Applied when leaving a faction so player cannot join or create another faction quickly.
                          Default value: 24 hours or 86400 seconds.
                          Min duration: 60 seconds. Max duration: 7 days (604800 seconds)."),
                    min: 60,
                    max: 7 * 24 * 60 * 60);

            SharedFactionLandClaimsPerLevel
                = (float)MathHelper.Clamp(
                    ServerRates.Get(
                        "Faction.LandClaimsPerLevel",
                        defaultValue: 1.2,
                        @"Determines how many land claims each faction level provides.
                          Total number is calculated as a faction level multiplied by this rate,
                          then rounded to the nearest integer number."),
                    min: 1,
                    max: 20);

            SharedPvpAlliancesEnabled
                = ServerRates.Get(
                      "Faction.PvP.AlliancesEnabled",
                      defaultValue: 1,
                      @"Determines whether the alliances list is available for PvP servers.
                        By default PvP alliances are allowed.
                        Change to 0 to disable alliances. Please note: already existing alliance will remain.")
                  == 1;
        }

        public static event Action ClientCreateFactionLearningPointsCostChanged;

        public static event Action ClientFactionJoinCooldownDurationChanged;

        public static event Action ClientFactionLandClaimsPerLevelChanged;

        public static event Action ClientFactionMembersMaxChanged;

        public static event Action ClientPvpAlliancesEnabledChanged;

        public static ushort SharedCreateFactionLearningPointsCost { get; private set; }

        public static uint SharedFactionJoinCooldownDuration { get; private set; }

        public static float SharedFactionLandClaimsPerLevel { get; private set; }

        public static bool SharedPvpAlliancesEnabled { get; private set; }

        public static void ClientSetSystemConstants(
            ushort publicFactionMembersMax,
            ushort privateFactionMembersMax,
            ushort createFactionLpCost,
            uint factionJoinCooldownDuration,
            float factionLandClaimsPerLevel,
            bool pvpAlliancesEnabled)
        {
            Api.ValidateIsClient();

            sharedPublicFactionMembersMax = publicFactionMembersMax;
            sharedPrivateFactionMembersMax = privateFactionMembersMax;
            Api.Logger.Info(
                string.Format(
                    "Faction member max size constants received from server. Public faction: {0}. Private faction: {1}",
                    publicFactionMembersMax,
                    privateFactionMembersMax));
            Api.SafeInvoke(ClientFactionMembersMaxChanged);

            SharedCreateFactionLearningPointsCost = createFactionLpCost;
            Api.Logger.Info("Faction creation LP cost constant received from server: "
                            + SharedCreateFactionLearningPointsCost);
            Api.SafeInvoke(ClientCreateFactionLearningPointsCostChanged);

            SharedFactionJoinCooldownDuration = factionJoinCooldownDuration;
            Api.Logger.Info("Faction join cooldown duration constant received from server: "
                            + SharedFactionJoinCooldownDuration);
            Api.SafeInvoke(ClientFactionJoinCooldownDurationChanged);

            SharedFactionLandClaimsPerLevel = factionLandClaimsPerLevel;
            Api.Logger.Info("Faction land claims number per level constant received from server: "
                            + SharedFactionLandClaimsPerLevel.ToString("0.##"));
            Api.SafeInvoke(ClientFactionLandClaimsPerLevelChanged);

            SharedPvpAlliancesEnabled = pvpAlliancesEnabled;
            Api.SafeInvoke(ClientPvpAlliancesEnabledChanged);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void EnsureInitialized()
        {
        }

        public static ushort GetFactionMembersMax(FactionKind kind)
        {
            return kind switch
            {
                FactionKind.Public  => sharedPublicFactionMembersMax,
                FactionKind.Private => sharedPrivateFactionMembersMax,
                _                   => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        public static int SharedGetFactionLandClaimsLimit(byte level)
        {
            return (int)Math.Round(SharedFactionLandClaimsPerLevel
                                   * MathHelper.Clamp((int)level, 1, MaxFactionLevel),
                                   MidpointRounding.AwayFromZero);
        }

        public static ushort SharedGetFactionUpgradeCost(byte toLevel)
        {
            if (toLevel > MaxFactionLevel)
            {
                throw new Exception("Cannot upgrade beyond level " + MaxFactionLevel);
            }

            return (ushort)(toLevel * 100);
        }
    }
}