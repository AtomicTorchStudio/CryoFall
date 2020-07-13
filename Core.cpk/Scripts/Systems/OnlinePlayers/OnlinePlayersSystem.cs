namespace AtomicTorch.CBND.CoreMod.Systems.OnlinePlayers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class OnlinePlayersSystem : ProtoSystem<OnlinePlayersSystem>
    {
        private static readonly HashSet<Entry> ClientOnlinePlayersList
            = Api.IsClient ? new HashSet<Entry>() : null;

        private static int serverLastTotalPlayersCount;

        public delegate void OnlinePlayedAddedOrRemoved(Entry entry, bool isOnline);

        public static event Action<Entry> ClientOnlinePlayerClanTagChanged;

        public static event OnlinePlayedAddedOrRemoved ClientPlayerAddedOrRemoved;

        public static event Action<int> ClientTotalServerPlayersCountChanged;

        public static int ClientOnlinePlayersCount => ClientOnlinePlayersList.Count;

        /// <summary>
        /// Please note: this property will return zero in case the player is not a server operator.
        /// </summary>
        public static int ClientTotalServerPlayersCount { get; private set; }

        public override string Name => "Online players system";

        public static bool ClientContains(string name)
        {
            return ClientOnlinePlayersList.Contains(new Entry(name, null));
        }

        public static IEnumerable<Entry> ClientEnumerateOnlinePlayers()
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
            Server.Characters.PlayerOnlineStateChanged += this.ServerOnPlayerOnlineStateChangedHandler;
            PartySystem.ServerCharacterJoinedOrLeftParty += this.ServerCharacterJoinedOrLeftPartyHandler;
            PartySystem.ServerPartyClanTagChanged += this.ServerPartyClanTagChanged;

            ServerRefreshTotalPlayersCount();
        }

        private static void ClientProcessPlayerStatusChange(Entry entry, bool isOnline)
        {
            var isChanged = isOnline
                                ? ClientOnlinePlayersList.Add(entry)
                                : ClientOnlinePlayersList.Remove(entry);
            if (isChanged)
            {
                ClientPlayerAddedOrRemoved?.Invoke(entry, isOnline);
            }
        }

        private static string ServerGetClanTag(ICharacter playerCharacter)
        {
            var clanTag = PlayerCharacter.GetPublicState(playerCharacter).ClanTag;
            return string.IsNullOrEmpty(clanTag)
                       ? null
                       : clanTag;
        }

        private static void ServerRefreshTotalPlayersCount()
        {
            var totalPlayersCount = Server.Characters.TotalPlayerCharactersCount;
            if (serverLastTotalPlayersCount == totalPlayersCount)
            {
                return;
            }

            serverLastTotalPlayersCount = totalPlayersCount;

            // provide the updated info about the total players count only to the server operators
            using var tempList = Api.Shared.GetTempList<ICharacter>();
            foreach (var character in Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true))
            {
                if (ServerOperatorSystem.ServerIsOperator(character.Name))
                {
                    tempList.Add(character);
                }
            }

            Instance.CallClient(tempList.AsList(),
                                _ => _.ClientRemote_TotalPlayerCharactersCountChanged(serverLastTotalPlayersCount));
        }

        private void ClientRemote_OnlineList(List<Entry> onlineList)
        {
            Logger.Important(
                "Online players list received from server: " + onlineList.GetJoinedString());

            if (ClientOnlinePlayersList.Count > 0)
            {
                foreach (var name in ClientOnlinePlayersList.ToList())
                {
                    ClientProcessPlayerStatusChange(name, isOnline: false);
                }

                ClientOnlinePlayersList.Clear();
            }

            foreach (var entry in onlineList)
            {
                ClientProcessPlayerStatusChange(entry, isOnline: true);
            }
        }

        private void ClientRemote_OnlinePlayerClanTagChanged(Entry entry)
        {
            ClientOnlinePlayerClanTagChanged?.Invoke(entry);
        }

        private void ClientRemote_OnlineStatusChanged(Entry entry, bool isOnline)
        {
            ClientProcessPlayerStatusChange(entry, isOnline);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_TotalPlayerCharactersCountChanged(int totalPlayerCharactersCount)
        {
            ClientTotalServerPlayersCount = totalPlayerCharactersCount;
            ClientTotalServerPlayersCountChanged?.Invoke(totalPlayerCharactersCount);
        }

        private void ServerCharacterJoinedOrLeftPartyHandler(ICharacter playerCharacter)
        {
            this.ServerForceRefreshPlayerEntry(playerCharacter);
        }

        private void ServerForceRefreshPlayerEntry(ICharacter playerCharacter)
        {
            var isOnline = playerCharacter.ServerIsOnline;
            if (!isOnline)
            {
                return;
            }

            var charactersDestination = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
            this.CallClient(charactersDestination.ExceptOne(playerCharacter),
                            _ => _.ClientRemote_OnlinePlayerClanTagChanged(
                                new Entry(playerCharacter.Name,
                                          ServerGetClanTag(playerCharacter))));
        }

        private void ServerOnPlayerOnlineStateChangedHandler(ICharacter playerCharacter, bool isOnline)
        {
            ServerRefreshTotalPlayersCount();

            var charactersDestination = Server.Characters
                                              .EnumerateAllPlayerCharacters(onlyOnline: true);

            // ReSharper disable once PossibleMultipleEnumeration
            this.CallClient(charactersDestination.ExceptOne(playerCharacter),
                            _ => _.ClientRemote_OnlineStatusChanged(
                                new Entry(playerCharacter.Name,
                                          ServerGetClanTag(playerCharacter)),
                                isOnline));

            if (!isOnline)
            {
                // player disconnected
                return;
            }

            // send to this character the list of online characters
            // ReSharper disable once PossibleMultipleEnumeration
            var onlineList = charactersDestination.Select(c => new Entry(c.Name,
                                                                         ServerGetClanTag(c)))
                                                  .ToList();

            // uncomment to test the online players list with some fake data
            //onlineList.Add(new Entry("Aaa1", "XXX"));
            //onlineList.Add(new Entry("Ссс1", "XXX"));
            //onlineList.Add(new Entry("Bbb1", "XXX"));
            //onlineList.Add(new Entry("Aaa", ""));
            //onlineList.Add(new Entry("Ссс", null));
            //onlineList.Add(new Entry("Bbb", ""));
            //onlineList.Add(new Entry("Aaa2", "YYY"));
            //onlineList.Add(new Entry("Ссс2", "YYY"));
            //onlineList.Add(new Entry("Bbb2", "YYY"));

            this.CallClient(playerCharacter, _ => _.ClientRemote_OnlineList(onlineList));

            // provide info about the total players count only to a server operator
            var totalPlayersCount = ServerOperatorSystem.ServerIsOperator(playerCharacter.Name)
                                        ? serverLastTotalPlayersCount
                                        : 0;

            this.CallClient(playerCharacter,
                            _ => _.ClientRemote_TotalPlayerCharactersCountChanged(totalPlayersCount));
        }

        private void ServerPartyClanTagChanged(ILogicObject party)
        {
            var serverCharacters = Server.Characters;
            foreach (var playerCharacterName in PartySystem.ServerGetPartyMembersReadOnly(party))
            {
                var playerCharacter = serverCharacters.GetPlayerCharacter(playerCharacterName);
                if (playerCharacter != null)
                {
                    this.ServerForceRefreshPlayerEntry(playerCharacter);
                }
            }
        }

        [NotPersistent]
        public readonly struct Entry : IRemoteCallParameter, IComparable<Entry>, IComparable, IEquatable<Entry>
        {
            public readonly string Name;

            private readonly string clanTag;

            public Entry(string name, string clanTag)
            {
                this.Name = name;
                this.clanTag = string.IsNullOrEmpty(clanTag)
                                   ? null
                                   : clanTag;
            }

            public string ClanTag => string.IsNullOrEmpty(this.clanTag)
                                         ? null
                                         : this.clanTag;

            public static int CompareOnlyName(Entry x, Entry y)
            {
                return StringComparer.Ordinal.Compare(x.Name, y.Name);
            }

            public static int CompareWithTag(Entry x, Entry y)
            {
                var xIsNull = x.ClanTag is null;
                var yIsNull = y.ClanTag is null;

                if (xIsNull && !yIsNull)
                {
                    return 1;
                }

                if (!xIsNull && yIsNull)
                {
                    return -1;
                }

                var compareTag = StringComparer.Ordinal.Compare(x.ClanTag, y.ClanTag);
                if (compareTag != 0)
                {
                    return compareTag;
                }

                return StringComparer.Ordinal.Compare(x.Name, y.Name);
            }

            public static bool operator >(Entry left, Entry right)
            {
                return left.CompareTo(right) > 0;
            }

            public static bool operator >=(Entry left, Entry right)
            {
                return left.CompareTo(right) >= 0;
            }

            public static bool operator <(Entry left, Entry right)
            {
                return left.CompareTo(right) < 0;
            }

            public static bool operator <=(Entry left, Entry right)
            {
                return left.CompareTo(right) <= 0;
            }

            public int CompareTo(Entry other)
            {
                return string.Compare(this.Name, other.Name, StringComparison.Ordinal);
            }

            public int CompareTo(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return 1;
                }

                return obj is Entry other
                           ? this.CompareTo(other)
                           : throw new ArgumentException($"Object must be of type {nameof(Entry)}");
            }

            public bool Equals(Entry other)
            {
                return this.Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                return obj is Entry other
                       && this.Equals(other);
            }

            public override int GetHashCode()
            {
                return (this.Name != null ? this.Name.GetHashCode() : 0);
            }

            public override string ToString()
            {
                return string.IsNullOrEmpty(this.ClanTag)
                           ? this.Name
                           : $"[{this.ClanTag}] {this.Name}";
            }
        }
    }
}