namespace AtomicTorch.CBND.CoreMod.Systems.ItemFreshnessSystem
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class ItemFreshnessConstants
    {
        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        static ItemFreshnessConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            const double minValue = 0.01,
                         maxValue = 100,
                         defaultValue = 1.0;
            var key = "FreshnessDecaySpeedMultiplier";
            var description = $@"Adjusts the speed at which the food and some other items lose their freshness.
                                 (allowed range: from {minValue:0.0###} to {maxValue:0.0###})";

            SharedFreshnessDecaySpeedMultiplier = ServerRates.Get(
                key,
                defaultValue: defaultValue,
                description);

            var clampedValue = MathHelper.Clamp(SharedFreshnessDecaySpeedMultiplier, minValue, maxValue);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (SharedFreshnessDecaySpeedMultiplier != clampedValue)
            {
                // incorrect value, reset to the vanilla value
                clampedValue = defaultValue;
                ServerRates.Reset(key, defaultValue, description);
            }

            SharedFreshnessDecaySpeedMultiplier = clampedValue;
        }

        public static double SharedFreshnessDecaySpeedMultiplier { get; private set; }

        public static void ClientSetSystemConstants(double freshnessDecaySpeedMultiplier)
        {
            Api.ValidateIsClient();
            SharedFreshnessDecaySpeedMultiplier = freshnessDecaySpeedMultiplier;
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void EnsureInitialized()
        {
        }
    }
}