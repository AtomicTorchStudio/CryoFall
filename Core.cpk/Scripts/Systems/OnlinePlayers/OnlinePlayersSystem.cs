namespace AtomicTorch.CBND.CoreMod.Systems.OnlinePlayers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.ServerModerator;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    [PrepareOrder(afterType: typeof(RatesSynchronizationSystem))]
    public class OnlinePlayersSystem : ProtoSystem<OnlinePlayersSystem>
    {
        private static readonly HashSet<Entry> ClientOnlinePlayersList
            = Api.IsClient ? new HashSet<Entry>() : null;

        private static int clientReceivedOnlinePlayersCountWhenListHidden;

        private static int serverLastTotalPlayersCount;

        public delegate void OnlinePlayedAddedOrRemoved(Entry entry, bool isOnline);

        public static event Action<Entry> ClientOnlinePlayerClanTagChanged;

        public static event Action<int> ClientOnlinePlayersCountChanged;

        public static event OnlinePlayedAddedOrRemoved ClientPlayerAddedOrRemoved;

        public static event Action<int> ClientTotalServerPlayersCountChanged;

        public static bool ClientIsReady { get; private set; }

        public static int ClientOnlinePlayersCount
            => SharedIsListHidden
                   ? clientReceivedOnlinePlayersCountWhenListHidden
                   : ClientOnlinePlayersList.Count + 1;

        /// <summary>
        /// Please note: this property will return zero in case the player is not a server operator.
        /// </summary>
        public static int ClientTotalServerPlayersCount { get; private set; }

        public static bool SharedIsListHidden
        {
            get
            {
                if (IsServer)
                {
                    return RateIsOnlinePlayersListHidden.SharedValue;
                }

                return RateIsOnlinePlayersListHidden.SharedValue
                       && !ServerOperatorSystem.ClientIsOperator()
                       && !ServerModeratorSystem.ClientIsModerator();
            }
        }

        public static IEnumerable<Entry> ClientEnumerateOnlinePlayers()
        {
            return ClientOnlinePlayersList;
        }

        public static bool ClientIsOnline(string name)
        {
            return ClientOnlinePlayersList.Contains(new Entry(name, null))
                   || name == ClientCurrentCharacterHelper.Character?.Name;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            // listen to player online state changed event
            Server.Characters.PlayerOnlineStateChanged += this.ServerOnPlayerOnlineStateChangedHandler;
            PartySystem.ServerCharacterJoinedOrLeftParty += this.ServerCharacterJoinedOrLeftPartyOrFactionHandler;
            FactionSystem.ServerCharacterJoinedOrLeftFaction += this.ServerCharacterJoinedOrLeftPartyOrFactionHandler;

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
                ClientOnlinePlayersCountChanged?.Invoke(ClientOnlinePlayersCount);
            }
        }

        private static List<ICharacter> ServerGetOnlineStatusChangeReceivers(ICharacter aboutPlayerCharacter)
        {
            var list = Server.Characters
                             .EnumerateAllPlayerCharacters(onlyOnline: true)
                             .ToList();

            for (var index = 0; index < list.Count; index++)
            {
                var character = list[index];
                if (ReferenceEquals(character, aboutPlayerCharacter))
                {
                    list.RemoveAt(index);
                    break;
                }
            }

            var aboutPlayerCharacterParty = PartySystem.ServerGetParty(aboutPlayerCharacter);
            string aboutPlayerCharacterFactionClanTag = null;
            {
                var faction = FactionSystem.ServerGetFaction(aboutPlayerCharacter);
                if (faction is not null)
                {
                    /*if (FactionSystem.SharedGetFactionKind(faction) == FactionKind.Public)
                    {
                        // public faction doesn't provide online status of faction members
                    }
                    else
                    {*/
                        aboutPlayerCharacterFactionClanTag = FactionSystem.SharedGetClanTag(faction);
                    //}
                }
            }

            List<ICharacter> onlineStatusChangeReceivers;
            if (SharedIsListHidden)
            {
                // only server operators, moderators, and party members will receive a notification
                onlineStatusChangeReceivers = new List<ICharacter>(list.Count);
                foreach (var character in list)
                {
                    if (ServerIsOperatorOrModerator(character)
                        || (aboutPlayerCharacterFactionClanTag is not null
                            && string.Equals(aboutPlayerCharacterFactionClanTag,
                                             FactionSystem.SharedGetClanTag(character)))
                        || (aboutPlayerCharacterParty is not null
                            && ReferenceEquals(aboutPlayerCharacterParty,
                                               PartySystem.ServerGetParty(character))))
                    {
                        onlineStatusChangeReceivers.Add(character);
                    }
                }
            }
            else
            {
                // all players will receive a notification about the status change for this player
                onlineStatusChangeReceivers = list;
            }

            return onlineStatusChangeReceivers;
        }

        private static bool ServerIsOperatorOrModerator(ICharacter playerCharacter)
        {
            var name = playerCharacter.Name;
            return ServerOperatorSystem.ServerIsOperator(name)
                   || ServerModeratorSystem.ServerIsModerator(name);
        }

        private static void ServerRefreshTotalPlayersCount()
        {
            var totalPlayersCount = Server.Characters.TotalPlayerCharactersCount;
            if (serverLastTotalPlayersCount == totalPlayersCount)
            {
                return;
            }

            serverLastTotalPlayersCount = totalPlayersCount;

            // provide the updated info about the total players count only to the server operators and moderators
            using var tempList = Api.Shared.GetTempList<ICharacter>();
            foreach (var character in Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true))
            {
                if (ServerIsOperatorOrModerator(character))
                {
                    tempList.Add(character);
                }
            }

            Instance.CallClient(tempList.AsList(),
                                _ => _.ClientRemote_TotalPlayerCharactersCountChanged(serverLastTotalPlayersCount));
        }

        private void ClientRemote_OnlinePlayerClanTagChanged(Entry entry)
        {
            ClientOnlinePlayerClanTagChanged?.Invoke(entry);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_OnlinePlayersCountWhenListHidden(int charactersOnlinePlayersCount)
        {
            clientReceivedOnlinePlayersCountWhenListHidden = charactersOnlinePlayersCount;
            ClientOnlinePlayersCountChanged?.Invoke(ClientOnlinePlayersCount);
        }

        private void ClientRemote_OnlinePlayersList(
            IReadOnlyList<Entry> list,
            int totalOnlineCountIfListHidden)
        {
            ClientIsReady = false;

            try
            {
                clientReceivedOnlinePlayersCountWhenListHidden = totalOnlineCountIfListHidden;

                Logger.Important(
                    "Online players list received from server."
                    + Environment.NewLine
                    + "Total entries: "
                    + list.Count
                    /*+ Environment.NewLine
                    + "List:"
                    + Environment.NewLine
                    + list.GetJoinedString()*/);

                if (ClientOnlinePlayersList.Count > 0)
                {
                    foreach (var name in ClientOnlinePlayersList.ToList())
                    {
                        ClientProcessPlayerStatusChange(name, isOnline: false);
                    }

                    ClientOnlinePlayersList.Clear();
                }

                foreach (var entry in list)
                {
                    ClientProcessPlayerStatusChange(entry, isOnline: true);
                }
            }
            finally
            {
                ClientIsReady = true;
            }
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

        private void ServerCharacterJoinedOrLeftPartyOrFactionHandler(
            ICharacter playerCharacter,
            ILogicObject party,
            bool isJoined)
        {
            if (!playerCharacter.ServerIsOnline)
            {
                // it's not in online players list so no notifications necessary
                return;
            }

            var onlineStatusChangeReceivers = ServerGetOnlineStatusChangeReceivers(playerCharacter);
            this.CallClient(onlineStatusChangeReceivers,
                            _ => _.ClientRemote_OnlinePlayerClanTagChanged(
                                new Entry(playerCharacter.Name,
                                          FactionSystem.SharedGetClanTag(playerCharacter))));

            if (!SharedIsListHidden)
            {
                return;
            }

            // need to notify current party and faction members that this player is online
            this.CallClient(onlineStatusChangeReceivers,
                            _ => _.ClientRemote_OnlineStatusChanged(
                                new Entry(playerCharacter.Name,
                                          FactionSystem.SharedGetClanTag(playerCharacter)),
                                true));
        }

        private void ServerOnPlayerOnlineStateChangedHandler(ICharacter playerCharacter, bool isOnline)
        {
            ServerRefreshTotalPlayersCount();

            var onlineStatusChangeReceivers = ServerGetOnlineStatusChangeReceivers(playerCharacter);
            this.CallClient(onlineStatusChangeReceivers,
                            _ => _.ClientRemote_OnlineStatusChanged(
                                new Entry(playerCharacter.Name,
                                          FactionSystem.SharedGetClanTag(playerCharacter)),
                                isOnline));

            if (SharedIsListHidden)
            {
                this.CallClient(Server.Characters
                                      .EnumerateAllPlayerCharacters(onlyOnline: true)
                                      .ExceptOne(playerCharacter)
                                      .Except(onlineStatusChangeReceivers),
                                _ => _.ClientRemote_OnlinePlayersCountWhenListHidden(
                                    Server.Characters.OnlinePlayersCount));
            }

            if (!isOnline)
            {
                // player disconnected
                return;
            }

            this.ServerSendOnlineListAndOtherInfo(playerCharacter);
        }

        private void ServerSendOnlineListAndOtherInfo(ICharacter playerCharacter)
        {
            if (!playerCharacter.ServerIsOnline)
            {
                return;
            }

            var isOperatorOrModerator = ServerIsOperatorOrModerator(playerCharacter);
            var isListHidden = SharedIsListHidden
                               && !isOperatorOrModerator;

            var onlinePlayersList = new List<Entry>();
            if (isListHidden)
            {
                // send only the party and faction members
                var faction = FactionSystem.ServerGetFaction(playerCharacter);
                if (faction is not null)
                    //&& FactionSystem.SharedGetFactionKind(faction) != FactionKind.Public)
                {
                    var factionClanTag = FactionSystem.SharedGetClanTag(faction);
                    onlinePlayersList.AddRange(
                        Server.Characters
                              .EnumerateAllPlayerCharacters(onlyOnline: true)
                              .ExceptOne(playerCharacter)
                              .Where(c => factionClanTag == FactionSystem.SharedGetClanTag(c))
                              .Select(c => new Entry(c.Name, factionClanTag)));
                }

                var party = PartySystem.ServerGetParty(playerCharacter);
                if (party is not null)
                {
                    var partyMembers = Server.Characters
                                             .EnumerateAllPlayerCharacters(onlyOnline: true)
                                             .ExceptOne(playerCharacter)
                                             .Where(c => party == PartySystem.ServerGetParty(c))
                                             .Select(c => new Entry(c.Name, FactionSystem.SharedGetClanTag(c)));
                    if (onlinePlayersList.Count == 0)
                    {
                        onlinePlayersList.AddRange(partyMembers);
                    }
                    else
                    {
                        foreach (var entry in partyMembers)
                        {
                            onlinePlayersList.AddIfNotContains(entry);
                        }
                    }
                }
            }
            else
            {
                onlinePlayersList = Server.Characters
                                          .EnumerateAllPlayerCharacters(onlyOnline: true)
                                          .ExceptOne(playerCharacter)
                                          .Select(c => new Entry(c.Name,
                                                                 FactionSystem.SharedGetClanTag(c)))
                                          .ToList();
            }

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

            this.CallClient(playerCharacter,
                            _ => _.ClientRemote_OnlinePlayersList(onlinePlayersList,
                                                                  isListHidden
                                                                      ? Server.Characters.OnlinePlayersCount
                                                                      : 0));

            // provide info about the total players count only to a server operator or moderator
            var totalPlayersCount = isOperatorOrModerator
                                        ? serverLastTotalPlayersCount
                                        : 0;

            this.CallClient(playerCharacter,
                            _ => _.ClientRemote_TotalPlayerCharactersCountChanged(totalPlayersCount));
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
                return (this.Name is not null ? this.Name.GetHashCode() : 0);
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