namespace AtomicTorch.CBND.CoreMod.Systems.ItemFreshnessSystem
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class ItemFreshnessConstants
    {
        public static readonly double ServerFreshnessDecaySpeedMultiplier;

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

            ServerFreshnessDecaySpeedMultiplier = ServerRates.Get(
                key,
                defaultValue: defaultValue,
                description);

            var clampedValue = MathHelper.Clamp(ServerFreshnessDecaySpeedMultiplier, minValue, maxValue);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (ServerFreshnessDecaySpeedMultiplier != clampedValue)
            {
                // incorrect value, reset to the vanilla value
                clampedValue = defaultValue;
                ServerRates.Reset(key, defaultValue, description);
            }

            ServerFreshnessDecaySpeedMultiplier = clampedValue;
        }

        public static double ClientFreshnessDecaySpeedMultiplier { get; private set; }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void EnsureInitialized()
        {
        }

        // This bootstrapper requests FreshnessDecaySpeedMultiplier rate value from server.
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

                    ClientFreshnessDecaySpeedMultiplier = 1.0;
                    ClientFreshnessDecaySpeedMultiplier =
                        await ItemFreshnessSystem.Instance.CallServer(
                            _ => _.ServerRemote_RequestFreshnessDecaySpeedMultiplier());
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                EnsureInitialized();
            }
        }
    }
}