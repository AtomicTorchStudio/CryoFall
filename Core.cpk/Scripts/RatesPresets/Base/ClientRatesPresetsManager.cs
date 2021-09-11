namespace AtomicTorch.CBND.CoreMod.RatesPresets.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public static class ClientRatesPresetsManager
    {
        static ClientRatesPresetsManager()
        {
            if (Api.IsServer)
            {
                return;
            }

            // it's essential to execute this in two steps
            // (the getter for OrderAfterPreset requires already initialized RatesPresets) 
            RatesPresets = Api.Shared
                              .FindScriptingTypes<BaseRatesPreset>()
                              .Select(t => t.CreateInstance())
                              .ToArray();

            RatesPresets = RatesPresets.OrderBy(r => r.OrderAfterPreset is null)
                                       .ThenBy(r => r.GetType().Name)
                                       .TopologicalSort(GetPresetOrder)
                                       .ToArray();

            foreach (var preset in RatesPresets)
            {
                try
                {
                    preset.ClientInit();
                }
                catch (Exception ex)
                {
                    Api.Logger.Exception(ex, "Cannot initialize server rate: " + preset);
                }
            }

            // local helper method for getting preset order
            static IEnumerable<BaseRatesPreset> GetPresetOrder(BaseRatesPreset rate)
            {
                if (rate.OrderAfterPreset is not null)
                {
                    yield return rate.OrderAfterPreset;
                }
            }
        }

        public static IReadOnlyList<BaseRatesPreset> RatesPresets { get; }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void EnsureInitialized()
        {
        }

        public static TRatesPreset GetInstance<TRatesPreset>()
            where TRatesPreset : BaseRatesPreset, new()
        {
            foreach (var preset in RatesPresets)
            {
                if (preset is TRatesPreset result)
                {
                    return result;
                }
            }

            throw new Exception("Unknown server rates preset: " + typeof(TRatesPreset));
        }
    }
}