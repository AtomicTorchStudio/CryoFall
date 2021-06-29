namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class LandClaimSystemConstants
    {
        static LandClaimSystemConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            SharedLandClaimOwnersMax = (byte)MathHelper.Clamp(
                ServerRates.Get(
                    "LandClaimOwnersMax",
                    // by default it's 5 owners only, if players wish to add more they can create a faction
                    defaultValue: 5,
                    @"This rate determines the max number of land claim's owners (including the founder)
                      for land claims that are not transferred to faction ownership.
                      If you want to make a server where players must group in factions,
                      set it to 1 and reduce the faction create cost.
                      Min value: 1 owner (no access list displayed).
                      Max value: 255 owners (no limit displayed)."),
                min: 1,
                max: byte.MaxValue);

            SharedRaidBlockDurationSeconds = MathHelper.Clamp(
                ServerRates.Get(
                    "PvP.RaidBlockDurationSeconds",
                    defaultValue: 10 * 60, // 10 minutes
                    @"This rate determines the raid block duration in PvP.
                      Min value: 0 seconds.
                      Max value: 3600 seconds (one hour)."),
                min: 0,
                max: 60 * 60);

            SharedLandClaimsNumberLimitIncrease = (ushort)MathHelper.Clamp(
                ServerRates.Get(
                    "LandClaimsNumberLimitIncrease",
                    defaultValue: SharedLocalServerHelper.IsLocalServer
                                      ? 10 // more land claims for local server
                                      : 0,
                    @"This rate determines the EXTRA number of land claims ANY player can own simultaneously.                                
                      Currently in the game every player can build max 3 land claims.
                      This rate allows to increase the number.
                      Please note: it doesn't apply to faction-controlled land claims (see Faction.LandClaimsPerLevel).
                      Min value: 0 (extra 0 land claims, that's the default value).
                      Max value: 50 (extra 50 land claims)."),
                min: 0,
                max: 50);
        }

        public static event Action ClientRaidBlockDurationSecondsChanged;

        public static byte SharedLandClaimOwnersMax { get; private set; } = byte.MaxValue;

        public static ushort SharedLandClaimsNumberLimitIncrease { get; private set; }

        /// <summary>
        /// Determines the duration of "raid block" feature - preventing players from
        /// repairing and building new structures after the bomb is exploded within their land claim area.
        /// Applies only to bombs (except mining charge).
        /// </summary>
        public static double SharedRaidBlockDurationSeconds { get; private set; }

        public static void ClientSetSystemConstants(
            byte landClaimOwnersMax,
            ushort landClaimsNumberLimitIncrease,
            double raidBlockDurationSeconds)
        {
            Api.ValidateIsClient();

            SharedLandClaimOwnersMax = landClaimOwnersMax;
            SharedLandClaimsNumberLimitIncrease = landClaimsNumberLimitIncrease;

            if (SharedLandClaimsNumberLimitIncrease > 0)
            {
                Api.GetProtoEntity<PlayerCharacter>()
                   .SharedReinitializeDefaultEffects();

                Api.Client.Characters.CurrentPlayerCharacter
                   .SharedSetFinalStatsCacheDirty();
            }

            SharedRaidBlockDurationSeconds = raidBlockDurationSeconds;
            Api.SafeInvoke(ClientRaidBlockDurationSecondsChanged);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static void EnsureInitialized()
        {
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;
                Refresh();

                static void Refresh()
                {
                    if (SharedLandClaimsNumberLimitIncrease > 0)
                    {
                        Api.GetProtoEntity<PlayerCharacter>()
                           .SharedReinitializeDefaultEffects();
                    }

                    SharedLandClaimsNumberLimitIncrease = 0;
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                EnsureInitialized();
            }
        }
    }
}