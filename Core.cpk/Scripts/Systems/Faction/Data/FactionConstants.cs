namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class FactionConstants
    {
        public const double FactionApplicationLifetimeSeconds = 48 * 60 * 60; // 48 hours

        public const double FactionInvitationLifetimeSeconds = 48 * 60 * 60; // 48 hours

        /// <summary>
        /// Maximum faction level.
        /// If you change it, please ensure that the code for "Faction.UpgradeCostPerLevel"
        /// below is adjusted accordingly.
        /// </summary>
        public const byte MaxFactionLevel = 10;

        static FactionConstants()
        {
            if (Api.IsServer)
            {
                return;
            }

            RateFactionMembersMaxPrivateFaction.ClientValueChanged += ClientNotifyFactionMembersMaxChanged;
            RateFactionMembersMaxPublicFaction.ClientValueChanged += ClientNotifyFactionMembersMaxChanged;
            RateFactionCreateCost.ClientValueChanged += ClientNotifyFactionCreateCostChanged;
        }

        public static event Action ClientFactionCreateCostChanged;

        public static event Action ClientFactionMembersMaxChanged;

        public static ushort SharedCreateFactionCost
            => RateFactionCreateCost.SharedValue;

        public static uint SharedFactionJoinCooldownDuration
            => RateFactionJoinCooldownDuration.SharedValue;

        public static uint SharedFactionJoinReturnBackCooldownDuration
            => RateFactionJoinReturnCooldownDuration.SharedValue;

        public static double SharedFactionLandClaimsPerLevel
            => RateFactionLandClaimsPerLevel.SharedValue;

        public static ushort[] SharedFactionUpgradeCosts
            => RateFactionUpgradeCostPerLevel.SharedFactionUpgradeCosts;

        public static ushort SharedMembersMaxPrivateFaction
            => RateFactionMembersMaxPrivateFaction.SharedValue;

        public static ushort SharedMembersMaxPublicFaction
            => RateFactionMembersMaxPublicFaction.SharedValue;

        public static bool SharedPvpAlliancesEnabled
            => RateFactionPvPAlliancesEnabled.SharedValue;

        public static int SharedGetFactionLandClaimsLimit(byte level)
        {
            return (int)Math.Round(SharedFactionLandClaimsPerLevel
                                   * MathHelper.Clamp((int)level, 1, MaxFactionLevel),
                                   MidpointRounding.AwayFromZero);
        }

        public static ushort SharedGetFactionMembersMax(FactionKind kind)
        {
            return kind switch
            {
                FactionKind.Public  => SharedMembersMaxPublicFaction,
                FactionKind.Private => SharedMembersMaxPrivateFaction,
                _                   => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        public static ushort SharedGetFactionUpgradeCost(byte toLevel)
        {
            if (toLevel > MaxFactionLevel)
            {
                throw new Exception("Cannot upgrade beyond level " + MaxFactionLevel);
            }

            // -2 is correct here (e.g. when faction upgrades to level 2 the cost of upgrade is at the index 0)
            return SharedFactionUpgradeCosts[toLevel - 2];
        }

        private static void ClientNotifyFactionCreateCostChanged()
        {
            Api.SafeInvoke(ClientFactionCreateCostChanged);
        }

        private static void ClientNotifyFactionMembersMaxChanged()
        {
            Api.SafeInvoke(ClientFactionMembersMaxChanged);
        }
    }
}