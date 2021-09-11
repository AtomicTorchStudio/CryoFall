namespace AtomicTorch.CBND.CoreMod.Rates
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    [PrepareOrder(afterType: typeof(IRate))]
    [NotPersistent]
    public class RatesSynchronizationSystem : ProtoEntity
    {
        private static RateEntry[] cachedServerRatesData;

        private static RatesSynchronizationSystem instance;

        static RatesSynchronizationSystem()
        {
            if (IsServer)
            {
                Server.Characters.PlayerOnlineStateChanged += ServerPlayerOnlineStateChangedHandler;
            }
        }

        public RatesSynchronizationSystem()
        {
            instance = this;
        }

        public override string Name => nameof(RatesSynchronizationSystem);

        public static void ClientConfigureRatesOnServer(IServerRatesConfig ratesConfig)
        {
            if (Client.CurrentGame.ConnectionState != ConnectionState.Connected)
            {
                throw new Exception("Not connected");
            }

            var address = Client.CurrentGame.ServerInfo.ServerAddress;
            var isLocalServer = address.IsLocalServer;
            if (!isLocalServer
                && !ServerOperatorSystem.SharedIsOperator(ClientCurrentCharacterHelper.Character))
            {
                throw new Exception(
                    "Cannot change rates for current server - the player must be a server operator or connect to a local server");
            }

            instance.CallServer(_ => _.ServerRemote_SetRatesConfig(ratesConfig.ToDictionary()));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void EnsureInitialized()
        {
        }

        private static RateEntry[] ServerGatherServerRates()
        {
            var protoServerRates = RatesManager.Rates;
            var result = new List<RateEntry>();
            foreach (var serverRate in protoServerRates)
            {
                result.Add(
                    new RateEntry(serverRate.Id,
                                  serverRate.SharedAbstractValue));
            }

            return result.ToArray();
        }

        private static void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            if (!isOnline)
            {
                return;
            }

            cachedServerRatesData ??= ServerGatherServerRates();

            instance.CallClient(character,
                                _ => _.ClientRemote_ServerRates(cachedServerRatesData));
        }

        private void ClientRemote_ServerRates(RateEntry[] serverRates)
        {
            /*Logger.Dev("Received server rates:"
                       + Environment.NewLine
                       + serverRates.Select(e => e.Id + "=" + e.AbstractValue)
                                    .GetJoinedString(Environment.NewLine));*/

            RatesManager.ClientSetValuesFromServer(serverRates);
        }

        [RemoteCallSettings(timeInterval: 3)]
        private void ServerRemote_SetRatesConfig(Dictionary<string, object> ratesDictionary)
        {
            var character = ServerRemoteContext.Character;
            if (!ServerOperatorSystem.SharedIsOperator(character)
                && !(Server.Core.IsLocalServer
                     && Server.Core.IsLocalServerHoster(character)))
            {
                throw new Exception("No access");
            }

            var ratesConfig = Server.Core.CreateNewServerRatesConfig();
            foreach (var serverRate in RatesManager.Rates)
            {
                if (ratesDictionary.TryGetValue(serverRate.Id, out var value))
                {
                    serverRate.SharedApplyToConfig(ratesConfig, value);
                }
            }

            if (Server.Core.ServerRatesConfig.IsMatches(ratesConfig))
            {
                Logger.Important("Server rates config change is not required");
                return;
            }

            Server.Game.SetServerConfig(ratesConfig);
            Logger.Important("Server rates config changed");
        }

        [NotPersistent]
        public readonly struct RateEntry : IRemoteCallParameter
        {
            public readonly object AbstractValue;

            public readonly string Id;

            public RateEntry(string id, object abstractValue)
            {
                this.Id = id;
                this.AbstractValue = abstractValue;
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                EnsureInitialized();
            }
        }
    }
}