namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
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
                value: ServerRates.Get(
                    key: "LandClaimOwnersMax",
                    defaultValue: byte.MaxValue,
                    description:
                    @"This rate determines the max number of land claim's owners (including the founder).
                      Min value: 1 owner.
                      Max value: 255 owners (no limit displayed)."),
                min: 1,
                max: byte.MaxValue);

            SharedRaidBlockDurationSeconds = MathHelper.Clamp(
                value: ServerRates.Get(
                    key: "RaidBlockDurationSeconds",
                    defaultValue: 10 * 60, // 10 minutes
                    description:
                    @"This rate determines the raid block duration in PvP.
                      Min value: 0 seconds.
                      Max value: 3600 seconds (one hour)."),
                min: 0,
                max: 60 * 60);

            SharedLandClaimsNumberLimitIncrease = (ushort)MathHelper.Clamp(
                value: ServerRates.Get(
                    key: "LandClaimsNumberLimitIncrease",
                    defaultValue: 0,
                    description:
                    @"This rate determines the EXTRA number of land claims ANY player can build.                                
                      Currently in the game every player can build max 3 land claims.
                      This rate allows to increase the number.
                      Please don't set it too high otherwise players might abuse this in order to block other players.
                      Min value: 0 (extra 0 land claims, that's the default value).
                      Max value: 50 (extra 50 land claims)."),
                min: 0,
                max: 50);
        }

        public static event Action ClientRaidBlockDurationSecondsChanged;

        public static ushort SharedLandClaimsNumberLimitIncrease { get; private set; }

        public static byte SharedLandClaimOwnersMax { get; private set; } = byte.MaxValue;

        /// <summary>
        /// Determines the duration of "raid block" feature - preventing players from
        /// repairing and building new structures after the bomb is exploded within their land claim area.
        /// Applies only to bombs (except mining charge).
        /// </summary>
        public static double SharedRaidBlockDurationSeconds { get; private set; }

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

                static async void Refresh()
                {
                    if (SharedLandClaimsNumberLimitIncrease > 0)
                    {
                        Api.GetProtoEntity<PlayerCharacter>()
                           .SharedReinitializeDefaultEffects();
                    }

                    SharedLandClaimsNumberLimitIncrease = 0;
                    if (Api.Client.Characters.CurrentPlayerCharacter is null)
                    {
                        return;
                    }

                    var (landClaimsNumberLimit, raidBlockDurationSeconds)
                        = await LandClaimSystem.Instance.CallServer(
                              _ => _.ServerRemote_RequestLandClaimSystemConstants());

                    SharedLandClaimsNumberLimitIncrease = landClaimsNumberLimit;

                    if (SharedLandClaimsNumberLimitIncrease > 0)
                    {
                        Api.GetProtoEntity<PlayerCharacter>()
                           .SharedReinitializeDefaultEffects();

                        Api.Client.Characters.CurrentPlayerCharacter
                           .SharedSetFinalStatsCacheDirty();
                    }

                    SharedRaidBlockDurationSeconds = raidBlockDurationSeconds;
                    Api.SafeInvoke(ClientRaidBlockDurationSecondsChanged);

                    SharedLandClaimOwnersMax =
                        await LandClaimSystem.Instance.CallServer(_ => _.ServerRemote_GetLandClaimOwnersMax());
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                EnsureInitialized();
            }
        }
    }
}