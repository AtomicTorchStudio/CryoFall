namespace AtomicTorch.CBND.CoreMod.Systems.OnlinePlayers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class OnlinePlayersSystem : ProtoSystem<OnlinePlayersSystem>
    {
        private static readonly HashSet<string> ClientOnlinePlayersList
            = Api.IsClient ? new HashSet<string>() : null;

        private static int serverLastTotalPlayersCount;

        public delegate void OnlinePlayedAddedOrRemoved(string name, bool isOnline);

        public static event OnlinePlayedAddedOrRemoved ClientOnPlayerAddedOrRemoved;

        public static event Action<int> ClientTotalServerPlayersCountChanged;

        public static int ClientTotalServerPlayersCount { get; private set; }

        public override string Name => "Online players system";

        public static IEnumerable<string> ClientEnumerateOnlinePlayers()
        {
            return ClientOnlinePlayersList;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            // listen to player online state changed event
            Server.Characters.PlayerOnlineStateChanged += ServerOnPlayerOnlineStateChangedHandler;

            ServerRefreshTotalPlayersCount();

            void ServerOnPlayerOnlineStateChangedHandler(ICharacter playerCharacter, bool isOnline)
            {
                ServerRefreshTotalPlayersCount();

                var name = playerCharacter.Name;
                var charactersDestination = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
                this.CallClient(charactersDestination.ExceptOne(playerCharacter).ToList(),
                                _ => _.ClientRemote_OnlineStatusChanged(name, isOnline));

                if (!isOnline)
                {
                    return;
                }

                // send to this character the list of online characters
                var onlineList = charactersDestination.Select(c => c.Name).ToList();
                this.CallClient(playerCharacter, _ => _.ClientRemote_OnlineList(onlineList));
                this.CallClient(playerCharacter,
                                _ => _.ClientRemote_TotalPlayerCharactersCountChanged(serverLastTotalPlayersCount));
            }
        }

        private static void ClientProcessChange(string name, bool isOnline)
        {
            var isChanged = isOnline
                                ? ClientOnlinePlayersList.Add(name)
                                : ClientOnlinePlayersList.Remove(name);
            if (isChanged)
            {
                ClientOnPlayerAddedOrRemoved?.Invoke(name, isOnline);
            }
        }

        private static void ServerRefreshTotalPlayersCount()
        {
            var totalPlayersCount = Server.Characters.TotalPlayerCharactersCount;
            if (serverLastTotalPlayersCount == totalPlayersCount)
            {
                return;
            }

            serverLastTotalPlayersCount = totalPlayersCount;
            // notify all the players online
            Instance.CallClient(Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                                _ => _.ClientRemote_TotalPlayerCharactersCountChanged(serverLastTotalPlayersCount));
        }

        private void ClientRemote_OnlineList(List<string> onlineList)
        {
            Logger.Important(
                "Online players list received from server: " + onlineList.GetJoinedString());

            if (ClientOnlinePlayersList.Count > 0)
            {
                foreach (var name in ClientOnlinePlayersList.ToList())
                {
                    ClientProcessChange(name, isOnline: false);
                }

                ClientOnlinePlayersList.Clear();
            }

            foreach (var name in onlineList)
            {
                ClientProcessChange(name, isOnline: true);
            }
        }

        private void ClientRemote_OnlineStatusChanged(string name, bool isOnline)
        {
            ClientProcessChange(name, isOnline);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_TotalPlayerCharactersCountChanged(int totalPlayerCharactersCount)
        {
            ClientTotalServerPlayersCount = totalPlayerCharactersCount;
            ClientTotalServerPlayersCountChanged?.Invoke(totalPlayerCharactersCount);
        }
    }
}