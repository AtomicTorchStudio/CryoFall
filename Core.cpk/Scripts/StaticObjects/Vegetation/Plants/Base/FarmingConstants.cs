namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Systems.ItemFreshnessSystem;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class FarmingConstants
    {
        public const double WateringGrowthSpeedMultiplier = 2.0;

        public static readonly double ServerFarmPlantsGrowthSpeedMultiplier;

        public static double SharedFarmPlantsSpoilSpeedMultiplier;

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
        }

        // This bootstrapper requests FarmPlantsSpoilSpeedMultiplier rate value from server.
        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;
                Refresh();

                async void Refresh()
                {
                    if (Api.Client.Characters.CurrentPlayerCharacter is null)
                    {
                        return;
                    }

                    SharedFarmPlantsSpoilSpeedMultiplier = 1.0;
                    SharedFarmPlantsSpoilSpeedMultiplier =
                        await ItemFreshnessSystem.Instance.CallServer(
                            _ => _.ServerRemote_RequestFarmPlantsSpoilSpeedMultiplier());
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                Logger.Important("Farm plants growth speed multiplier: "
                                 + ServerFarmPlantsGrowthSpeedMultiplier.ToString("0.###")
                                 + Environment.NewLine
                                 + "Farm plants spoil speed multiplier: "
                                 + SharedFarmPlantsSpoilSpeedMultiplier.ToString("0.###"));
            }
        }
    }
}