namespace AtomicTorch.CBND.CoreMod.Systems.Party
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class PartyMembersVisualizationSystem : ProtoSystem<PartyMembersVisualizationSystem>
    {
        private static readonly ICharactersServerService CharactersServerService
            = IsServer ? Server.Characters : null;

        private static readonly TimeSpan ServerRefreshInterval
            = TimeSpan.FromSeconds(1);

        private static bool clientIsEnabled;

        private readonly List<ICharacter> subscribedCharacters
            = new List<ICharacter>();

        public static event Action<IReadOnlyList<ClientPartyMemberData>> ClientUpdateReceived;

        public static bool ClientIsEnabled
        {
            get => clientIsEnabled;
            set
            {
                if (clientIsEnabled == value)
                {
                    return;
                }

                clientIsEnabled = value;

                if (PartySystem.ClientCurrentParty != null)
                {
                    Instance.CallServer(_ => _.ServerRemote_SetSubscription(clientIsEnabled));
                }
            }
        }

        [NotLocalizable]
        public override string Name => "Party members visualization system";

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
        }

        [RemoteCallSettings(DeliveryMode.UnreliableSequenced)]
        private void ClientRemote_Update(NetworkPartyMemberData[] data)
        {
            if (ClientUpdateReceived == null)
            {
                return;
            }

            var partyMembers = PartySystem.ClientGetCurrentPartyMembers();
            if (data.Length != partyMembers.Count - 1)
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

            // ReSharper disable once AccessToDisposedClosure
            Api.SafeInvoke(() => ClientUpdateReceived?.Invoke(tempList));
        }

        private void ServerRefresh()
        {
            for (var index = 0; index < this.subscribedCharacters.Count; index++)
            {
                var character = this.subscribedCharacters[index];
                if (!character.ServerIsOnline)
                {
                    // offline player - stop updating
                    this.subscribedCharacters.RemoveAt(index);
                    index--;
                    continue;
                }

                var partyMembers = PartySystem.ServerGetPartyMembersReadOnly(character);
                if (partyMembers.Count <= 1)
                {
                    // don't have a party or an empty party - stop updating
                    this.subscribedCharacters.RemoveAt(index);
                    index--;
                    continue;
                }

                this.ServerSendUpdate(character, partyMembers);
            }
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

            var partyMembers = PartySystem.ServerGetPartyMembersReadOnly(character);
            if (partyMembers.Count <= 1)
            {
                // don't have a party or an empty party - no need to subscribe
                return;
            }

            this.subscribedCharacters.Add(character);
            this.ServerSendUpdate(character, partyMembers); // send immediate update
        }

        private void ServerSendUpdate(ICharacter character, IReadOnlyList<string> partyMembers)
        {
            var currentCharacterName = character.Name;
            var result = new NetworkPartyMemberData[partyMembers.Count - 1];
            var index = 0;
            var worldOffset = Server.World.WorldBounds.Offset;

            foreach (var partyMemberName in partyMembers)
            {
                if (partyMemberName == currentCharacterName)
                {
                    continue;
                }

                var partyMember = CharactersServerService.GetPlayerCharacter(partyMemberName);
                if (partyMember == null)
                {
                    // incorrect party, contains a null character
                    Logger.Warning(
                        $"Player has an incorrect party (it contains a null character): {character} member not found: {partyMemberName}");
                    return;
                }

                var position = partyMember.TilePosition - worldOffset;
                result[index++] = new NetworkPartyMemberData(position);
            }

            this.CallClient(character, _ => _.ClientRemote_Update(result));
        }

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