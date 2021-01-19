namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class FarmingConstants
    {
        public const double WateringGrowthSpeedMultiplier = 2.0;

        public static readonly double ServerFarmPlantsGrowthSpeedMultiplier;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        static FarmingConstants()
        {
            if (Api.IsClient)
            {
                ServerFarmPlantsGrowthSpeedMultiplier = 1.0;
                SharedFarmPlantsSpoilSpeedMultiplier = 1.0;
                return;
            }

            ServerFarmPlantsGrowthSpeedMultiplier = ServerRates.Get(
                "FarmPlantsGrowthSpeedMultiplier",
                defaultValue: 1.0,
                @"This rate determines how fast the farm plants grow.
                  (it doesn't apply to the already planted plants until harvested, watered, or fertilizer applied)");

            const double minValue = 0.01,
                         maxValue = 100,
                         defaultValue = 1.0;
            var key = "FarmPlantsSpoilSpeedMultiplier";
            var description = $@"This rate determines how fast the farm plants spoil.
                                 To make plants spoil twice as slow set it 0.5, to make them spoil twice as fast set it to 2.0.
                                 (allowed range: from {minValue:0.0###} to {maxValue:0.0###})
                                 (it doesn't apply to the already planted plants until harvested, watered, or fertilizer applied)";

            SharedFarmPlantsSpoilSpeedMultiplier = ServerRates.Get(
                key,
                defaultValue: defaultValue,
                description);

            var clampedValue = MathHelper.Clamp(SharedFarmPlantsSpoilSpeedMultiplier, minValue, maxValue);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (SharedFarmPlantsSpoilSpeedMultiplier != clampedValue)
            {
                // incorrect value, reset to the vanilla value
                clampedValue = defaultValue;
                ServerRates.Reset(key, defaultValue, description);
            }

            SharedFarmPlantsSpoilSpeedMultiplier = clampedValue;

            Api.Logger.Important("Farm plants growth speed multiplier: "
                                 + ServerFarmPlantsGrowthSpeedMultiplier.ToString("0.###")
                                 + Environment.NewLine
                                 + "Farm plants spoil speed multiplier: "
                                 + SharedFarmPlantsSpoilSpeedMultiplier.ToString("0.###"));
        }

        public static double SharedFarmPlantsSpoilSpeedMultiplier { get; private set; }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static void EnsureInitialized()
        {
        }

        private class FarmingConstantsProvider : ProtoSystem<FarmingConstantsProvider>
        {
            public override string Name => nameof(FarmingConstantsProvider);

            protected override void PrepareSystem()
            {
                EnsureInitialized();
                if (IsClient)
                {
                    return;
                }

                Server.Characters.PlayerOnlineStateChanged += ServerPlayerOnlineStateChangedHandler;
            }

            private static void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
            {
                if (isOnline)
                {
                    Instance.CallClient(character,
                                        _ => _.ClientRemote_SetSystemConstants(SharedFarmPlantsSpoilSpeedMultiplier));
                }
            }

            private void ClientRemote_SetSystemConstants(double sharedFarmPlantsSpoilSpeedMultiplier)
            {
                SharedFarmPlantsSpoilSpeedMultiplier = sharedFarmPlantsSpoilSpeedMultiplier;
            }
        }
    }
}