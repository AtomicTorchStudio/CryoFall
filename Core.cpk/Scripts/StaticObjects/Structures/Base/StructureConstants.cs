namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures
{
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class StructureConstants
    {
        // Please note that changing this value will require distribution
        // of a client+server mod so it will know about this change.
        public const byte BuildItemsCountMultiplier = 1;

        // Please note that changing this value will require distribution
        // of a client+server mod so it will know about this change.
        public const byte RepairItemsCountMultiplier = 1;

        /// <summary>
        /// Refresh rate of the land claim objects for the decay reset check.
        /// The game server should check whether there are any land claim owners inside the land claim area
        /// and reset the decay timer if so.
        /// </summary>
        public const double StructureDecayLandClaimResetSystemUpdateIntervalSeconds = 5;

        /// <summary>
        /// Interval with which the decay system will process the objects decay.
        /// Should not be too short as it will reduce the structure points too often
        /// (causing unnecessary network replication, spending more CPU time, and also having granularity issues).
        /// Should not be too large as it will reduce the precision.
        /// </summary>
        public const double StructureDecaySystemUpdateIntervalSeconds = 5 * 60; // every 5 minutes

        public static readonly double DepositsExtractionSpeedMultiplier;

        /// <summary>
        /// The resource can be claimed only if it was spawned longer than defined here.
        /// </summary>
        public static readonly int DepositsSpawnClaimingCooldownDuration;

        public static readonly bool IsStructuresDecayEnabled;

        public static readonly double ManufacturingSpeedMultiplier;

        public static readonly double StructuresAbandonedDecayDelaySeconds;

        public static readonly double StructuresDecayDurationSeconds;

        public static readonly double StructuresLandClaimDecayDelayDurationMultiplier;

        public static readonly double StructuresLandClaimDecayDelayDurationMultiplierForDemoPlayers;

        static StructureConstants()
        {
            IsStructuresDecayEnabled =
                ServerRates.Get(
                    "StructuresDecayEnabled",
                    defaultValue: Api.IsEditor ? 0 : 1,
                    @"Set it to 0 to disable the structures decay.
                    Set it to 1 to enable the structures decay.")
                > 0;

            StructuresAbandonedDecayDelaySeconds = ServerRates.Get(
                "StructuresAbandonedDecayDelaySeconds",
                defaultValue: 32 * 60 * 60,
                @"Time (in real world seconds) before an abandoned structure (without the land claim) will start decaying.
                  Default value: 32 hours or 115200 seconds. Don't set it higher than 2 billions.");

            StructuresLandClaimDecayDelayDurationMultiplier = ServerRates.Get(
                "StructuresLandClaimDecayDelayDurationMultiplier",
                defaultValue: 1.0,
                @"Time multiplier before an abandoned land claim (or base) will start decaying.
                  For example, the default decay delay for the land claims (T1) is 32 hours,
                  but with 2.0 multiplier it will be increased to 64 hours (for T1).");

            StructuresLandClaimDecayDelayDurationMultiplierForDemoPlayers = ServerRates.Get(
                "StructuresLandClaimDecayDelayDurationMultiplierForDemoPlayers",
                defaultValue: 0.875,
                @"(for demo players only)
                  Time multiplier before an abandoned land claim (or base) will start decaying.
                  For example, the default decay delay for the land claims (T1) is 32 hours,
                  but with 2.0 multiplier it will be increased to 64 hours (for T1).");

            if (StructuresLandClaimDecayDelayDurationMultiplierForDemoPlayers
                > StructuresLandClaimDecayDelayDurationMultiplier)
            {
                StructuresLandClaimDecayDelayDurationMultiplierForDemoPlayers =
                    StructuresLandClaimDecayDelayDurationMultiplier;
                Api.Logger.Error(
                    $"Please note: {nameof(StructuresLandClaimDecayDelayDurationMultiplierForDemoPlayers)} server rate value is higher than {nameof(StructuresLandClaimDecayDelayDurationMultiplier)}. The game has reduced it to match {nameof(StructuresLandClaimDecayDelayDurationMultiplier)} but it would be better if you correct the server rates config.");
            }

            StructuresDecayDurationSeconds = ServerRates.Get(
                "StructuresDecayDurationSeconds",
                defaultValue: 24 * 60 * 60,
                @"Time (in real world seconds) for a structure to completely destroy by decay.
                  While decaying, the structure will receive a regular damage proportional to its HP.
                  It also means that if the structure is already damaged, it will decay even faster.
                  Default value: 24 hours or 86400 seconds. Don't set it higher than 2 billions.");

            ManufacturingSpeedMultiplier = ServerRates.Get(
                "ManufacturingSpeedMultiplier",
                defaultValue: 1.0,
                @"Manufacturing rate for all manufacturers (such as furnaces, oil refineries, wells, mulchboxes, etc.)");

            DepositsExtractionSpeedMultiplier = ServerRates.Get(
                "DepositsExtractionSpeedMultiplier",
                defaultValue: 1.0,
                @"Deposits extraction rate for oil/Li extractors.
                  Please note: it also changes the power/fuel consumption of the deposit extractors.");

            DepositsSpawnClaimingCooldownDuration = ServerRates.Get(
                "DepositsSpawnClaimingCooldownDuration",
                defaultValue: 20 * 60, // 20 minutes,
                @"Delay in seconds before the spawned resource deposit could be claimed on a PvP server.
                  The notification about the spawned resource is displayed with this timer.
                  30 minutes by default (1800 seconds).
                  If you change this to 0 there would be no resource spawn notification (only a map mark will be added).");

            if (Api.IsServer)
            {
                SharedDoorOwnersMax = (byte)MathHelper.Clamp(
                    value: ServerRates.Get(
                        key: "DoorOwnersMax",
                        defaultValue: byte.MaxValue,
                        description:
                        @"This rate determines the max number of door's owners (including the builder).
                      Min value: 1 owner.
                      Max value: 255 owners (no limit displayed)."),
                    min: 1,
                    max: byte.MaxValue);
            }
        }

        public static byte SharedDoorOwnersMax { get; private set; }

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
                    SharedDoorOwnersMax = byte.MaxValue;
                    if (Api.Client.Characters.CurrentPlayerCharacter == null)
                    {
                        return;
                    }

                    SharedDoorOwnersMax =
                        await WorldObjectOwnersSystem.Instance.CallServer(_ => _.ServerRemote_GetDoorOwnersMax());
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                EnsureInitialized();
            }
        }
    }
}