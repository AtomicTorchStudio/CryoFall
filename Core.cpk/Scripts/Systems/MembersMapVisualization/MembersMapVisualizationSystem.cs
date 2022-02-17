namespace AtomicTorch.CBND.CoreMod.Systems.MembersMapVisualization
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class MembersMapVisualizationSystem : ProtoSystem<MembersMapVisualizationSystem>
    {
        private static readonly ICharactersServerService CharactersServerService
            = IsServer ? Server.Characters : null;

        // It's just a hashset of objects to track the number of controls subscribed on party member location info.
        private static readonly HashSet<object> ClientEnabledForObjects
            = new();

        private static readonly TimeSpan ServerRefreshInterval
            = TimeSpan.FromSeconds(1);

        private static bool clientIsEnabled;

        private readonly List<ICharacter> subscribedCharacters
            = new();

        public static event Action<IReadOnlyList<ClientPartyMemberData>> ClientUpdateReceived;

        private static bool ClientIsEnabled
        {
            get => clientIsEnabled;
            set
            {
                if (clientIsEnabled == value)
                {
                    return;
                }

                clientIsEnabled = value;

                if (PartySystem.ClientCurrentParty is not null
                    || FactionSystem.ClientCurrentFaction is not null)
                {
                    Instance.CallServer(_ => _.ServerRemote_SetSubscription(clientIsEnabled));
                }
            }
        }

        public static void ClientDisableFor(object key)
        {
            ClientEnabledForObjects.Remove(key);
            ClientIsEnabled = ClientEnabledForObjects.Count > 0;
        }

        public static void ClientEnableFor(object key)
        {
            ClientEnabledForObjects.Add(key);
            var wasEnabled = ClientIsEnabled;
            ClientIsEnabled = true;

            if (wasEnabled
                && (PartySystem.ClientCurrentParty is not null
                    || FactionSystem.ClientCurrentFaction is not null))
            {
                // ensure client requesting an update ASAP to quickly display party members on the map
                // (minimal delay)
                Instance.CallServer(_ => _.ServerRemote_RefreshNow());
            }
        }

        protected override void PrepareSystem()
        {
            base.PrepareSystem();

            if (IsServer)
            {
                TriggerTimeInterval.ServerConfigureAndRegister(
                    ServerRefreshInterval,
                    this.ServerRefresh,
                    $"{this.ShortId}.{nameof(this.ServerRefresh)}");
            }
            else
            {
                PartySystem.ClientCurrentPartyChanged += this.ClientCurrentPartyOrFactionChangedHandler;
                FactionSystem.ClientCurrentFactionChanged += this.ClientCurrentPartyOrFactionChangedHandler;
            }
        }

        private void ClientCurrentPartyOrFactionChangedHandler()
        {
            if (clientIsEnabled)
            {
                // re-initialize to subscribe for the event
                ClientIsEnabled = false;
                ClientIsEnabled = true;
            }
        }

        [RemoteCallSettings(DeliveryMode.UnreliableSequenced)]
        private void ClientRemote_Update(NetworkPartyMemberData[] data)
        {
            if (ClientUpdateReceived is null)
            {
                return;
            }

            var partyMembers = PartySystem.ClientGetCurrentPartyMembers();
            var factionMembers = FactionSystem.ClientCurrentFaction is not null
                                     /*&& FactionSystem.ClientCurrentFactionKind != FactionKind.Public*/
                                     ? FactionSystem.ClientGetCurrentFactionMembers()
                                     : Array.Empty<FactionMemberEntry>();

            if (partyMembers.Count <= 1
                && factionMembers.Count <= 1)
            {
                return;
            }

            if (data.Length
                != (Math.Max(partyMembers.Count,     1)
                    + Math.Max(factionMembers.Count, 1)
                    - 2))
            {
                // incorrect data - doesn't match the current members list (excluding the current player)
                return;
            }

            var index = 0;
            var currentCharacterName = ClientCurrentCharacterHelper.Character.Name;
            var worldOffset = Client.World.WorldBounds.Offset;

            using var tempList = Api.Shared.GetTempList<ClientPartyMemberData>();
            foreach (var partyMemberName in partyMembers)
            {
                if (partyMemberName == currentCharacterName)
                {
                    continue;
                }

                var entry = data[index++];
                tempList.Add(new ClientPartyMemberData(
                                 partyMemberName,
                                 // offset the position
                                 (entry.Position + worldOffset).ToVector2Ushort()));
            }

            foreach (var factionMemberEntry in factionMembers)
            {
                var factionMemberName = factionMemberEntry.Name;
                if (factionMemberName == currentCharacterName)
                {
                    continue;
                }

                var entry = data[index++];
                tempList.Add(new ClientPartyMemberData(
                                 factionMemberName,
                                 // offset the position
                                 (entry.Position + worldOffset).ToVector2Ushort()));
            }

            // ReSharper disable once AccessToDisposedClosure
            Api.SafeInvoke(() => ClientUpdateReceived?.Invoke(tempList.AsList()));
        }

        private void ServerRefresh()
        {
            for (var index = 0; index < this.subscribedCharacters.Count; index++)
            {
                var character = this.subscribedCharacters[index];
                if (character.ServerIsOnline)
                {
                    this.ServerSendUpdate(character);
                    continue;
                }

                // offline player - stop updating
                this.subscribedCharacters.RemoveAt(index);
                index--;
            }
        }

        [RemoteCallSettings(DeliveryMode.UnreliableSequenced, clientMaxSendQueueSize: 1)]
        private void ServerRemote_RefreshNow()
        {
            this.ServerSendUpdate(ServerRemoteContext.Character);
        }

        private void ServerRemote_SetSubscription(bool isEnabled)
        {
            var character = ServerRemoteContext.Character;
            if (!isEnabled)
            {
                this.subscribedCharacters.Remove(character);
                return;
            }

            if (this.subscribedCharacters.Contains(character))
            {
                // already subscribed
                return;
            }

            this.subscribedCharacters.Add(character);
            this.ServerSendUpdate(character);
        }

        private void ServerSendUpdate(ICharacter character)
        {
            var partyMembers = PartySystem.ServerGetPartyMembersReadOnly(character);
            var faction = FactionSystem.ServerGetFaction(character);
            var factionMembers = faction is not null
                                     /*&& Faction.GetPublicState(faction).Kind != FactionKind.Public*/
                                     ? FactionSystem.ServerGetFactionMembersReadOnly(faction)
                                     : Array.Empty<FactionMemberEntry>();

            if (partyMembers.Count <= 1
                && factionMembers.Count <= 1)
            {
                return;
            }

            var currentCharacterName = character.Name;
            var result = new NetworkPartyMemberData[Math.Max(partyMembers.Count,     1)
                                                    + Math.Max(factionMembers.Count, 1)
                                                    - 2];

            var index = 0;
            var worldOffset = Server.World.WorldBounds.Offset;

            foreach (var partyMemberName in partyMembers)
            {
                if (partyMemberName == currentCharacterName)
                {
                    continue;
                }

                var partyMember = CharactersServerService.GetPlayerCharacter(partyMemberName);
                if (partyMember is null)
                {
                    // incorrect party, contains a null character
                    Logger.Warning(
                        $"Player has an incorrect party (it contains a null character): {character} member not found: {partyMemberName}");
                    return;
                }

                var position = partyMember.TilePosition - worldOffset;
                result[index++] = new NetworkPartyMemberData(position);
            }

            foreach (var factionMemberEntry in factionMembers)
            {
                var factionMemberName = factionMemberEntry.Name;
                if (factionMemberName == currentCharacterName)
                {
                    continue;
                }

                var factionMember = CharactersServerService.GetPlayerCharacter(factionMemberName);
                if (factionMember is null)
                {
                    // incorrect faction, contains a null character
                    Logger.Warning(
                        $"Player has an incorrect faction (it contains a null character): {character} member not found: {factionMemberEntry}");
                    return;
                }

                var position = factionMember.TilePosition - worldOffset;
                result[index++] = new NetworkPartyMemberData(position);
            }

            this.CallClient(character, _ => _.ClientRemote_Update(result));
        }

        [NotPersistent]
        public readonly struct ClientPartyMemberData
        {
            public readonly string Name;

            public readonly Vector2Ushort Position;

            public ClientPartyMemberData(string name, in Vector2Ushort position)
            {
                this.Position = position;
                this.Name = name;
            }
        }

        [NotPersistent]
        public readonly struct NetworkPartyMemberData : IRemoteCallParameter
        {
            public readonly Vector2Ushort Position;

            public NetworkPartyMemberData(in Vector2Ushort position)
            {
                this.Position = position;
            }
        }
    }
}