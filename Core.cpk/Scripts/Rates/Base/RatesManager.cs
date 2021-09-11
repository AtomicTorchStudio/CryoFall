namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public static class RatesManager
    {
        static RatesManager()
        {
            Rates = Api.Shared.FindScriptingTypes<IRate>()
                       .Select(t => t.CreateInstance())
                       .OrderBy(t => t.Id)
                       .ToArray();

            if (Api.IsClient)
            {
                return;
            }

            foreach (var serverRate in Rates)
            {
                try
                {
                    serverRate.ServerInit();
                }
                catch (Exception ex)
                {
                    Api.Logger.Exception(ex, "Cannot initialize server rate: " + serverRate);
                }
            }
        }

        public static event Action ClientRatesReceived;

        public static IReadOnlyList<IRate> Rates { get; }

        public static void ClientSetValuesFromServer(RatesSynchronizationSystem.RateEntry[] receivedServerRates)
        {
            Dictionary<string, RatesSynchronizationSystem.RateEntry> dictionary;
            try
            {
                dictionary = receivedServerRates.ToDictionary(e => e.Id);
            }
            catch (ArgumentException)
            {
                dictionary = receivedServerRates.GroupBy(e => e.Id)
                                                .ToDictionary(g => g.Key, g => g.First());
                Api.Logger.Error("Found a duplicating server rate setting(s) Id in received settings:"
                                 + Environment.NewLine
                                 + receivedServerRates.GroupBy(e => e.Id)
                                                      .Where(g => g.Count() > 1)
                                                      .Select(e => e.Key)
                                                      .GetJoinedString());
            }

            foreach (var serverRate in Rates)
            {
                if (dictionary.TryGetValue(serverRate.Id, out var entry))
                {
                    serverRate.ClientSetAbstractValue(entry.AbstractValue);
                }
            }

            Api.SafeInvoke(ClientRatesReceived);
        }

        public static TServerRate GetInstance<TServerRate>()
            where TServerRate : IRate, new()
        {
            foreach (var entry in Rates)
            {
                if (entry is TServerRate result)
                {
                    return result;
                }
            }

            throw new Exception("Unknown server rate type: " + typeof(TServerRate));
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static void SharedEnsureInitialized()
        {
        }
    }
}