namespace AtomicTorch.CBND.CoreMod.Systems.Party
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.OnlinePlayers;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using JetBrains.Annotations;

    public class PartySystem : ProtoSystem<PartySystem>
    {
        public const string Notification_PlayerAcceptedInviteFormat
            = "{0} joined the party by invitation from {1}";

        public const string Notification_PlayerInvitedFormat
            = "{0} has invited {1} to join the party";

        public const string Notification_PlayerLeftPartyFormat
            = "{0} left the party";

        public const string Notification_PlayerRemovedPartyMemberFormat
            = "{0} was removed from the party by {1}";

        public const string Notification_RemovedFromTheParty
            = "{0} has removed you from the party.";

        public static readonly ObservableCollection<string> ClientCurrentInvitationsFromCharacters
            = IsClient
                  ? new ObservableCollection<string>()
                  : null;

        public static ushort ClientPartyMembersMax;

        public static Action ClientPartyMembersMaxChanged;

        private static readonly Dictionary<ICharacter, ILogicObject> ServerCharacterPartyDictionary
            = IsServer
                  ? new Dictionary<ICharacter, ILogicObject>()
                  : null;

        private static NetworkSyncList<string> clientCurrentPartyMembersList;

        public PartySystem()
        {
            PartyConstants.EnsureInitialized();
        }

        public delegate void ServerCharacterJoinedOrLeftPartyDelegate(
            ICharacter character,
            ILogicObject party,
            bool isJoined);

        public static event Action ClientCurrentPartyChanged;

        public static event Action<(string name, bool isAdded)> ClientCurrentPartyMemberAddedOrRemoved;

        public static event ServerCharacterJoinedOrLeftPartyDelegate ServerCharacterJoinedOrLeftParty;

        public static event Action<ILogicObject> ServerPartyClanTagChanged;

        public enum InvitationAcceptResult : byte
        {
            Unknown = 0,

            Success = 1,

            [Description("No invitation found or invitation has expired.")]
            ErrorNoInvitationFoundOrExpired = 2,

            [Description("Too late, the party is already full.")]
            ErrorPartyFull = 3
        }

        public enum InvitationCreateResult : byte
        {
            Unknown = 0,

            Success = 1,

            [Description("Invitee is not found.")]
            ErrorInviteeNotFound = 2,

            [Description("Party is full.")]
            ErrorPartyFull = 3,

            [Description("Invitee is already a member of your party.")]
            ErrorInviteeAlreadySamePartyMember = 4,

            [Description("You cannot invite, as you're muted by the server operator.")]
            ErrorMuted = 5,

            [Description("The invited player is offline. Only online players can be invited into a party.")]
            ErrorInviteeOffline = 6
        }

        public static ILogicObject ClientCurrentParty { get; private set; }

        [NotLocalizable]
        public override string Name => "Party system";

        public static void ClientCreateParty()
        {
            Api.Assert(ClientCurrentParty is null, "Already have a party");
            Instance.CallServer(_ => _.ServerRemote_CreateParty());
        }

        [NotNull]
        public static IReadOnlyList<string> ClientGetCurrentPartyMembers()
        {
            return clientCurrentPartyMembersList
                   ?? (IReadOnlyList<string>)Array.Empty<string>();
        }

        public static async void ClientInvitationAccept(string inviterName)
        {
            ClientCurrentInvitationsFromCharacters.Remove(inviterName);

            var result = await Instance.CallServer(
                             _ => _.ServerRemote_InvitationAccept(inviterName));

            if (result != InvitationAcceptResult.Success)
            {
                NotificationSystem.ClientShowNotification(
                    title: null,
                    result.GetDescription(),
                    NotificationColor.Bad);
            }
        }

        public static void ClientInvitationDecline(string inviterName)
        {
            ClientCurrentInvitationsFromCharacters.Remove(inviterName);
            Instance.CallServer(_ => _.ServerRemote_InvitationDecline(inviterName));
        }

        public static async void ClientInviteMember(string inviteeName)
        {
            if (ClientIsPartyMember(inviteeName))
            {
                ProcessResult(InvitationCreateResult.ErrorInviteeAlreadySamePartyMember);
                return;
            }

            ProcessResult(
                await Instance.CallServer(
                    _ => _.ServerRemote_InviteMember(inviteeName)));

            void ProcessResult(InvitationCreateResult result)
            {
                if (result == InvitationCreateResult.Success)
                {
                    ClientPartyInvitationNotificationSystem.ShowNotificationInviteSent(inviteeName);
                    return;
                }

                NotificationSystem.ClientShowNotification(
                    title: null,
                    result.GetDescription(),
                    NotificationColor.Bad);
            }
        }

        public static bool ClientIsPartyMember(string name)
        {
            return clientCurrentPartyMembersList?.Contains(name) ?? false;
        }

        public static void ClientLeaveParty()
        {
            Api.Assert(ClientCurrentParty is not null, "Don't have a party");
            Instance.CallServer(_ => _.ServerRemote_LeaveParty());
        }

        public static void ClientRemovePartyMember(string memberName)
        {
            Api.Assert(ClientCurrentParty is not null, "Don't have a party");
            Api.Assert(clientCurrentPartyMembersList[0] == ClientCurrentCharacterHelper.Character.Name,
                       "You're not the party owner");
            Instance.CallServer(_ => _.ServerRemote_RemovePartyMember(memberName));
        }

        public static Task<bool> ClientSetClanTag(string clanTag)
        {
            return Instance.CallServer(_ => _.ServerRemote_SetClanTag(clanTag));
        }

        public static ILogicObject ServerCreateParty(ICharacter character)
        {
            Api.Assert(!character.IsNpc, "NPC cannot create a party");

            var party = ServerGetParty(character);
            if (party is not null)
            {
                throw new Exception($"Player already has a party: {character} in {party}");
            }

            party = Server.World.CreateLogicObject<Party>();
            Logger.Important($"Created party: {party} for {character}", character);
            ServerAddMember(character, party);
            return party;
        }

        public static ILogicObject ServerFindPartyByClanTag(string clanTag)
        {
            if (string.IsNullOrEmpty(clanTag))
            {
                return null;
            }

            var allParties = Server.World.GetGameObjectsOfProto<ILogicObject, Party>();
            foreach (var party in allParties)
            {
                if (string.Equals(clanTag,
                                  Party.GetPublicState(party).ClanTag,
                                  StringComparison.Ordinal))
                {
                    return party;
                }
            }

            return null;
        }

        [CanBeNull]
        public static ILogicObject ServerGetParty(ICharacter character)
        {
            return ServerCharacterPartyDictionary.Find(character);
        }

        public static ILogicObject ServerGetPartyChat(ILogicObject party)
        {
            return Party.GetPrivateState(party)
                        .ServerPartyChatHolder;
        }

        public static IReadOnlyList<string> ServerGetPartyMembersReadOnly(ICharacter character)
        {
            var party = ServerGetParty(character);
            return party is not null
                       ? ServerGetPartyMembersReadOnly(party)
                       : Array.Empty<string>();
        }

        public static IReadOnlyList<string> ServerGetPartyMembersReadOnly(ILogicObject party)
        {
            return party is not null
                       ? ServerGetPartyMembersEditable(party)
                       : (IReadOnlyList<string>)Array.Empty<string>();
        }

        public static InvitationAcceptResult ServerInvitationAccept(ICharacter invitee, ICharacter inviter)
        {
            return ServerInvitations.Accept(invitee, inviter);
        }

        public static void ServerInvitationDecline(ICharacter invitee, ICharacter inviter)
        {
            ServerInvitations.Decline(invitee, inviter);
        }

        public static InvitationCreateResult ServerInviteMember(ICharacter invitee, ICharacter inviter)
        {
            return ServerInvitations.AddInvitation(invitee, inviter);
        }

        public static bool ServerIsSameParty(ICharacter characterA, ICharacter characterB)
        {
            var partyA = ServerGetParty(characterA);
            if (partyA is null)
            {
                return false;
            }

            var partyB = ServerGetParty(characterB);
            return ReferenceEquals(partyA, partyB);
        }

        public static void ServerLeaveParty(ICharacter character)
        {
            var party = ServerGetParty(character);
            if (party is null)
            {
                throw new Exception("Not a member of any party");
            }

            ServerRemoveMember(character.Name, party);
            var messageLeftParty = string.Format(Notification_PlayerLeftPartyFormat, character.Name);
            ChatSystem.ServerSendServiceMessage(ServerGetPartyChat(party),
                                                messageLeftParty,
                                                forCharacterName: character.Name);

            SendPrivateChatServiceMessage(character, party, messageLeftParty);
        }

        public static bool ServerSetClanTag(ILogicObject party, string clanTag)
        {
            var members = ServerGetPartyMembersReadOnly(party);
            if (string.IsNullOrEmpty(clanTag))
            {
                clanTag = null;
            }

            var partyPublicState = Party.GetPublicState(party);
            if (string.Equals(partyPublicState.ClanTag, clanTag, StringComparison.Ordinal))
            {
                // this party already has this clan tag
                return true;
            }

            if (!SharedIsValidClanTag(clanTag))
            {
                throw new Exception("Invalid clantag");
            }

            if (ClanTagFilterHelper.IsInvalidClanTag(clanTag))
            {
                Logger.Warning("Attempting to set a filter containing profanity or reserved word, ignoring it: "
                               + clanTag);
                clanTag = null;
                if (string.Equals(partyPublicState.ClanTag, null, StringComparison.Ordinal))
                {
                    // this party already has empty clan tag
                    return true;
                }
            }

            var otherParty = ServerFindPartyByClanTag(clanTag);
            if (otherParty is not null)
            {
                // the clan tag is already taken
                return false;
            }

            partyPublicState.ClanTag = clanTag;

            foreach (var memberName in members)
            {
                var member = Server.Characters.GetPlayerCharacter(memberName);
                if (member is null)
                {
                    Logger.Warning("Unknown character in party: " + memberName);
                    continue;
                }

                PlayerCharacter.GetPublicState(member).ClanTag = clanTag;
            }

            Api.SafeInvoke(
                () => ServerPartyClanTagChanged?.Invoke(party));

            return true;
        }

        public static bool SharedArePlayersInTheSameParty(ICharacter characterA, ICharacter characterB)
        {
            if (characterA is null
                || characterB is null
                || characterA.IsNpc
                || characterB.IsNpc)
            {
                return false;
            }

            if (characterA == characterB)
            {
                return true;
            }

            if (IsServer)
            {
                return ServerGetParty(characterA)
                       == ServerGetParty(characterB);
            }

            // on the client the check is more hard as we have to determine who is the current client's character
            // and then check the party members list
            if (characterA.IsCurrentClientCharacter)
            {
                return ClientGetCurrentPartyMembers()
                    .Contains(characterB.Name, StringComparer.Ordinal);
            }

            if (characterB.IsCurrentClientCharacter)
            {
                return ClientGetCurrentPartyMembers()
                    .Contains(characterA.Name, StringComparer.Ordinal);
            }

            // client cannot check parties for other players
            return false;
        }

        public static string SharedGetClanTag(ILogicObject party)
        {
            return party is null
                       ? null
                       : Party.GetPublicState(party).ClanTag;
        }

        public static bool SharedIsValidClanTag(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            if (text.Length > 4)
            {
                return false;
            }

            text = text.ToUpperInvariant();
            if (!IsValidLetter(text[0]))
            {
                return false;
            }

            foreach (var c in text)
            {
                if (!char.IsDigit(c)
                    && !IsValidLetter(c))
                {
                    return false;
                }
            }

            return true;

            bool IsValidLetter(char c)
                => c >= 'A'
                   && c <= 'Z';
        }

        internal static void ServerRegisterParty(ILogicObject party)
        {
            var members = ServerGetPartyMembersReadOnly(party);

            foreach (var memberName in members)
            {
                var character = Server.Characters.GetPlayerCharacter(memberName);
                if (character is null)
                {
                    Logger.Warning("Unknown character in party: " + memberName);
                    continue;
                }

                if (ServerCharacterPartyDictionary.TryGetValue(character, out var existingParty))
                {
                    Logger.Warning($"Player is already in party: {memberName} from {existingParty} - will remove",
                                   character);
                    ServerRemoveMember(memberName, existingParty);
                }

                ServerCharacterPartyDictionary[character] = party;
            }
        }

        private static void ClientCurrentPartyMemberInsertedHandler(
            NetworkSyncList<string> source,
            int index,
            string value)
        {
            Api.SafeInvoke(
                () => ClientCurrentPartyMemberAddedOrRemoved?.Invoke((value, isAdded: true)));
        }

        private static void ClientCurrentPartyMemberRemovedHandler(
            NetworkSyncList<string> source,
            int index,
            string removedValue)
        {
            Api.SafeInvoke(
                () => ClientCurrentPartyMemberAddedOrRemoved?.Invoke((removedValue, isAdded: false)));
        }

        private static void ClientSetCurrentParty(ILogicObject party)
        {
            Logger.Important("Current party received: " + (party?.ToString() ?? "<no party>"));
            if (ClientCurrentParty == party)
            {
                return;
            }

            var handler = ClientCurrentPartyMemberAddedOrRemoved;
            var previousPartyMembersList = clientCurrentPartyMembersList;
            if (clientCurrentPartyMembersList is not null)
            {
                clientCurrentPartyMembersList.ClientElementInserted -= ClientCurrentPartyMemberInsertedHandler;
                clientCurrentPartyMembersList.ClientElementRemoved -= ClientCurrentPartyMemberRemovedHandler;
                clientCurrentPartyMembersList = null;
            }

            if (handler is not null
                && previousPartyMembersList is not null)
            {
                foreach (var member in previousPartyMembersList)
                {
                    Api.SafeInvoke(() => handler((member, isAdded: false)));
                }
            }

            ClientCurrentParty = party;
            clientCurrentPartyMembersList = party is not null
                                                ? Party.GetPrivateState(party).Members
                                                : null;

            if (clientCurrentPartyMembersList is not null)
            {
                clientCurrentPartyMembersList.ClientElementInserted += ClientCurrentPartyMemberInsertedHandler;
                clientCurrentPartyMembersList.ClientElementRemoved += ClientCurrentPartyMemberRemovedHandler;
            }

            Api.SafeInvoke(() => ClientCurrentPartyChanged?.Invoke());

            if (handler is not null
                && clientCurrentPartyMembersList is not null)
            {
                foreach (var member in clientCurrentPartyMembersList)
                {
                    Api.SafeInvoke(() => handler((member, isAdded: true)));
                }
            }
        }

        private static void SendPrivateChatServiceMessage(
            ICharacter aboutCharacter,
            ILogicObject party,
            string message)
        {
            var serverCharacters = Server.Characters;
            foreach (var memberName in ServerGetPartyMembersReadOnly(party))
            {
                var memberCharacter = serverCharacters.GetPlayerCharacter(memberName);
                if (memberCharacter is null
                    || ReferenceEquals(aboutCharacter, memberCharacter))
                {
                    continue;
                }

                var privateChat = ChatSystem.ServerFindPrivateChat(aboutCharacter, memberName);
                if (privateChat is not null)
                {
                    ChatSystem.ServerSendServiceMessage(privateChat,
                                                        message,
                                                        forCharacterName: aboutCharacter.Name,
                                                        customDestinationCharacter: memberCharacter);
                }
            }
        }

        private static void ServerAddMember(ICharacter character, ILogicObject party)
        {
            Api.Assert(!character.IsNpc, "NPC cannot join a party");

            var currentParty = ServerGetParty(character);
            if (currentParty == party)
            {
                // already in party
                return;
            }

            if (currentParty is not null)
            {
                throw new Exception($"Player already has a party: {character} in {party}");
            }

            var members = ServerGetPartyMembersEditable(party);
            var partyMembersMax = PartyConstants.ServerPartyMembersMax;
            if (members.Count >= partyMembersMax)
            {
                throw new Exception("Party size exceeded - max " + partyMembersMax);
            }

            members.Add(character.Name);
            ServerCharacterPartyDictionary[character] = party;
            PlayerCharacter.GetPublicState(character).ClanTag = Party.GetPublicState(party).ClanTag;
            Logger.Important($"Player joined party: {character} in {party}", character);

            ServerInvitations.RemoveAllInvitationsFor(character);

            Api.SafeInvoke(
                () => ServerCharacterJoinedOrLeftParty?.Invoke(character,
                                                               party,
                                                               isJoined: true));

            // add this with some delay to prevent from the bug when the player name listed twice due to the late delta-replication
            ServerTimersSystem.AddAction(delaySeconds: 0.1,
                                         () => ServerSendCurrentParty(character));
        }

        private static NetworkSyncList<string> ServerGetPartyMembersEditable(ILogicObject party)
        {
            return Party.GetPrivateState(party)
                        .Members;
        }

        private static void ServerPlayerNameChangedHandler(string oldName, string newName)
        {
            foreach (var partyEntry in ServerCharacterPartyDictionary.Values)
            {
                var members = Party.GetPrivateState(partyEntry).Members;

                for (var index = 0; index < members.Count; index++)
                {
                    var memberName = members[index];
                    if (!string.Equals(memberName, oldName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    members[index] = newName;
                    Logger.Important($"Replaced party member username: {oldName}->{newName} in {partyEntry}");
                    break;
                }
            }
        }

        private static void ServerRemoveMember(string characterName, ILogicObject party)
        {
            var members = ServerGetPartyMembersEditable(party);
            for (var index = 0; index < members.Count; index++)
            {
                var memberName = members[index];
                if (!string.Equals(memberName, characterName, StringComparison.Ordinal))
                {
                    continue;
                }

                // member found
                members.RemoveAt(index);
                Logger.Important($"Party member removed: {characterName} from {party}");

                var character = Server.Characters.GetPlayerCharacter(characterName);
                if (character is not null)
                {
                    ServerCharacterPartyDictionary.Remove(character);

                    PlayerCharacter.GetPublicState(character).ClanTag = null;

                    // remove all requests by this character
                    ServerInvitations.RemoveAllInvitationsBy(character);

                    Api.SafeInvoke(
                        () => ServerCharacterJoinedOrLeftParty?.Invoke(character,
                                                                       party,
                                                                       isJoined: false));

                    Server.World.ForceExitScope(character, party);
                    // send "no party" for this player
                    ServerSendCurrentParty(character);
                    ChatSystem.ServerRemoveChatRoomFromPlayerScope(character, ServerGetPartyChat(party));
                }

                if (members.Count == 0)
                {
                    ServerRemoveParty(party);
                }

                return;
            }

            Logger.Warning($"Party member is not found: {characterName} in {party}");
        }

        private static void ServerRemoveParty(ILogicObject party)
        {
            var members = ServerGetPartyMembersReadOnly(party);
            if (members.Count > 0)
            {
                // ensure all members have been removed
                while (members.Count > 0)
                {
                    ServerRemoveMember(members[0], party);
                }
            }

            if (party.IsDestroyed)
            {
                return;
            }

            Logger.Important("Party destroyed: " + party);
            Party.GetPublicState(party).ClanTag = null;
            Server.World.DestroyObject(ServerGetPartyChat(party));
            Server.World.DestroyObject(party);
        }

        private static void ServerSendCurrentParty(ICharacter character)
        {
            var party = ServerGetParty(character);
            if (party is null)
            {
                Instance.CallClient(character, _ => _.ClientRemote_CurrentParty(null));
                return;
            }

            ChatSystem.ServerAddChatRoomToPlayerScope(character, ServerGetPartyChat(party));

            Server.World.EnterPrivateScope(character, party);
            Server.World.ForceEnterScope(character, party);

            Instance.CallClient(character, _ => _.ClientRemote_CurrentParty(party));
        }

        private void ClientRemote_CurrentParty([CanBeNull] ILogicObject party)
        {
            ClientSetCurrentParty(party);
        }

        private void ClientRemote_InvitationAdded(string inviterName)
        {
            ClientCurrentInvitationsFromCharacters.Add(inviterName);
        }

        private void ClientRemote_InvitationRemoved(string inviterName)
        {
            ClientCurrentInvitationsFromCharacters.Remove(inviterName);
        }

        private void ClientRemote_RemovedFromParty(string ownerName)
        {
            NotificationSystem.ClientShowNotification(
                                  title: CoreStrings.PartyManagement_Title,
                                  message: string.Format(Notification_RemovedFromTheParty, ownerName))
                              .HideAfterDelay(60);
        }

        [RemoteCallSettings(timeInterval: 5)]
        private void ServerRemote_CreateParty()
        {
            ServerCreateParty(ServerRemoteContext.Character);
        }

        private InvitationAcceptResult ServerRemote_InvitationAccept(string inviterName)
        {
            try
            {
                var inviter = Server.Characters.GetPlayerCharacter(inviterName);
                if (inviter is null)
                {
                    throw new Exception("Inviter player not found: " + inviterName);
                }

                return ServerInvitationAccept(invitee: ServerRemoteContext.Character, inviter);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return InvitationAcceptResult.Unknown;
            }
        }

        private void ServerRemote_InvitationDecline(string inviterName)
        {
            var inviter = Server.Characters.GetPlayerCharacter(inviterName);
            if (inviter is null)
            {
                throw new Exception("Inviter player not found: " + inviterName);
            }

            ServerInvitationDecline(invitee: ServerRemoteContext.Character, inviter);
        }

        [RemoteCallSettings(timeInterval: 2)]
        private InvitationCreateResult ServerRemote_InviteMember(string inviteeName)
        {
            try
            {
                var invitee = Server.Characters.GetPlayerCharacter(inviteeName);
                if (invitee is null)
                {
                    return InvitationCreateResult.ErrorInviteeNotFound;
                }

                if (!invitee.ServerIsOnline)
                {
                    if (!OnlinePlayersSystem.ServerIsListHidden)
                    {
                        return InvitationCreateResult.ErrorInviteeOffline;
                    }

                    // In case of a hidden online players list server cannot disclose whether the invitee is offline
                    // so the server will allow invitations to offline players though they will expire quickly.
                }

                return ServerInviteMember(invitee, inviter: ServerRemoteContext.Character);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return InvitationCreateResult.Unknown;
            }
        }

        private void ServerRemote_LeaveParty()
        {
            ServerLeaveParty(ServerRemoteContext.Character);
        }

        // only party owner could remove other players
        private void ServerRemote_RemovePartyMember(string memberName)
        {
            var character = ServerRemoteContext.Character;
            var party = ServerGetParty(character);
            if (party is null)
            {
                return;
            }

            var members = ServerGetPartyMembersReadOnly(party);
            var ownerName = character.Name;
            if (members[0] != ownerName)
            {
                throw new Exception("You're not the party owner");
            }

            ServerRemoveMember(memberName, party);
            var messageRemovedFromParty =
                string.Format(Notification_PlayerRemovedPartyMemberFormat, memberName, ownerName);
            ChatSystem.ServerSendServiceMessage(
                ServerGetPartyChat(party),
                messageRemovedFromParty,
                forCharacterName: ownerName);

            SendPrivateChatServiceMessage(character, party, messageRemovedFromParty);

            var member = Server.Characters.GetPlayerCharacter(memberName);
            this.CallClient(member,
                            _ => _.ClientRemote_RemovedFromParty(ownerName));
        }

        [RemoteCallSettings(timeInterval: RemoteCallSettingsAttribute.MaxTimeInterval)]
        private void ServerRemote_RequestCurrentPartyAndInvitations()
        {
            var character = ServerRemoteContext.Character;
            ServerSendCurrentParty(character);

            ServerInvitations.ResendAllInvitations(invitee: character);
        }

        [RemoteCallSettings(timeInterval: RemoteCallSettingsAttribute.MaxTimeInterval)]
        private ushort ServerRemote_RequestPartyMaxSize()
        {
            return PartyConstants.ServerPartyMembersMax;
        }

        [RemoteCallSettings(timeInterval: 10)]
        private bool ServerRemote_SetClanTag(string clanTag)
        {
            var party = ServerGetParty(ServerRemoteContext.Character);
            if (party is null)
            {
                return true;
            }

            var members = ServerGetPartyMembersReadOnly(party);
            var ownerName = ServerRemoteContext.Character.Name;
            if (members[0] != ownerName)
            {
                throw new Exception("You're not the party owner");
            }

            return ServerSetClanTag(party, clanTag);
        }

        private static class ServerInvitations
        {
            private static readonly List<Invitation> InvitationsList
                = IsServer
                      ? new List<Invitation>()
                      : null;

            public static InvitationAcceptResult Accept(ICharacter invitee, ICharacter inviter)
            {
                var party = ServerGetParty(inviter);
                var invitation = GetInvitation(party, invitee);
                if (invitation is null)
                {
                    return InvitationAcceptResult.ErrorNoInvitationFoundOrExpired;
                }

                // should never happen
                Api.Assert(party is not null, "Inviter must have a party");
                var currentInviteeParty = ServerGetParty(invitee);
                Api.Assert(party != currentInviteeParty, "Cannot join the same party");

                var inviterPartyMembers = ServerGetPartyMembersReadOnly(party);
                if (inviterPartyMembers.Count >= PartyConstants.ServerPartyMembersMax)
                {
                    return InvitationAcceptResult.ErrorPartyFull;
                }

                if (currentInviteeParty is not null)
                {
                    // invitee already has a party - leave it first
                    ServerLeaveParty(invitee);
                }

                RemoveInvitation(invitation, notifyInvitee: false);
                RemoveAllInvitationsFor(invitee);

                var messageJoinedParty = string.Format(Notification_PlayerAcceptedInviteFormat,
                                                       invitee.Name,
                                                       inviter.Name);
                ChatSystem.ServerSendServiceMessage(ServerGetPartyChat(party),
                                                    messageJoinedParty,
                                                    forCharacterName: invitee.Name);

                SendPrivateChatServiceMessage(invitee, party, messageJoinedParty);

                ServerAddMember(invitee, party);

                return InvitationAcceptResult.Success;
            }

            public static InvitationCreateResult AddInvitation(ICharacter invitee, ICharacter inviter)
            {
                if (ServerPlayerMuteSystem.IsMuted(inviter.Name, out _))
                {
                    return InvitationCreateResult.ErrorMuted;
                }

                var party = ServerGetParty(inviter)
                            ?? ServerCreateParty(inviter);

                var invitation = GetInvitation(party, invitee);
                if (invitation is not null)
                {
                    invitation.ResetExpirationDate();
                    Logger.Info(
                        $"{invitee} is already has been invited by {inviter} to {party} - invitation timeout extended");
                    return InvitationCreateResult.Success;
                }

                var members = ServerGetPartyMembersReadOnly(party);
                if (members.Contains(invitee.Name, StringComparer.Ordinal))
                {
                    return InvitationCreateResult.ErrorInviteeAlreadySamePartyMember;
                }

                var partyMembers = ServerGetPartyMembersReadOnly(party);
                if (partyMembers.Count >= PartyConstants.ServerPartyMembersMax)
                {
                    return InvitationCreateResult.ErrorPartyFull;
                }

                invitation = new Invitation(party, inviter, invitee);
                InvitationsList.Add(invitation);

                Instance.CallClient(invitation.Invitee,
                                    _ => _.ClientRemote_InvitationAdded(invitation.Inviter.Name));
                Logger.Important($"{invitee} has been invited by {inviter} to {party}");

                ChatSystem.ServerSendServiceMessage(
                    ServerGetPartyChat(party),
                    string.Format(Notification_PlayerInvitedFormat, inviter.Name, invitee.Name),
                    invitee.Name);

                return InvitationCreateResult.Success;
            }

            public static void Decline(ICharacter invitee, ICharacter inviter)
            {
                var party = ServerGetParty(inviter);
                if (party is null)
                {
                    throw new Exception("Player don't have a party");
                }

                var invitation = GetInvitation(party, invitee);
                if (invitation is not null)
                {
                    RemoveInvitation(invitation, notifyInvitee: false);
                }
            }

            public static void RemoveAllInvitationsBy(ICharacter inviter)
            {
                for (var index = 0; index < InvitationsList.Count; index++)
                {
                    var invitation = InvitationsList[index];
                    if (invitation.Inviter != inviter)
                    {
                        continue;
                    }

                    InvitationsList.RemoveAt(index);
                    index--;

                    NotifyInvitationRemoved(invitation);
                }
            }

            public static void RemoveAllInvitationsFor(ICharacter invitee)
            {
                for (var index = 0; index < InvitationsList.Count; index++)
                {
                    var invitation = InvitationsList[index];
                    if (invitation.Invitee != invitee)
                    {
                        continue;
                    }

                    InvitationsList.RemoveAt(index);
                    index--;

                    NotifyInvitationRemoved(invitation);
                }
            }

            // please note - this is safe to execute only once - when client is just connected
            public static void ResendAllInvitations(ICharacter invitee)
            {
                foreach (var invitation in InvitationsList)
                {
                    if (invitation.Invitee == invitee)
                    {
                        Instance.CallClient(invitation.Invitee,
                                            _ => _.ClientRemote_InvitationAdded(invitation.Inviter.Name));
                    }
                }
            }

            // invoked once per second
            public static void UpdateInvitationsExpiration()
            {
                var currentTime = Server.Game.FrameTime;
                for (var index = 0; index < InvitationsList.Count; index++)
                {
                    var invitation = InvitationsList[index];
                    if (invitation.ExpirationDate > currentTime)
                    {
                        // Not expired.
                        // Please note we don't return here.
                        // It could be a nice optimization but not applicable here as
                        // the invitation expiration date could be extended.
                        continue;
                    }

                    // expired
                    InvitationsList.RemoveAt(index);
                    index--;

                    NotifyInvitationRemoved(invitation);
                }
            }

            private static Invitation GetInvitation(ILogicObject party, ICharacter invitee)
            {
                foreach (var invitation in InvitationsList)
                {
                    if (invitation.ToParty == party
                        && invitation.Invitee == invitee)
                    {
                        return invitation;
                    }
                }

                return null;
            }

            private static void NotifyInvitationRemoved(Invitation invitation)
            {
                Instance.CallClient(invitation.Invitee,
                                    _ => _.ClientRemote_InvitationRemoved(invitation.Inviter.Name));
            }

            private static void RemoveInvitation(Invitation invitation, bool notifyInvitee)
            {
                Api.Assert(invitation is not null, "Invitation cannot be null");
                InvitationsList.Remove(invitation);

                if (notifyInvitee)
                {
                    NotifyInvitationRemoved(invitation);
                }
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;

                Refresh();

                void Refresh()
                {
                    ClientSetCurrentParty(null);
                    ClientCurrentInvitationsFromCharacters.Clear();

                    if (Api.Client.Characters.CurrentPlayerCharacter is not null)
                    {
                        Instance.CallServer(_ => _.ServerRemote_RequestCurrentPartyAndInvitations());
                        Instance.CallServer(_ => _.ServerRemote_RequestPartyMaxSize())
                                .ContinueWith(t =>
                                              {
                                                  ClientPartyMembersMax = t.Result;
                                                  Logger.Info("Party member max size received from server: "
                                                              + ClientPartyMembersMax);
                                                  Api.SafeInvoke(ClientPartyMembersMaxChanged);
                                              },
                                              TaskContinuationOptions.ExecuteSynchronously);
                    }
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                // doesn't work here as GetGameObjectsOfProto is not available during bootstrap
                //var allParties = Server.World.GetGameObjectsOfProto<ILogicObject, Party>();
                //foreach (var party in allParties)
                //{
                //    ServerRegisterParty(party);
                //}

                Server.Characters.PlayerNameChanged += ServerPlayerNameChangedHandler;

                TriggerTimeInterval.ServerConfigureAndRegister(TimeSpan.FromSeconds(10),
                                                               ServerInvitations.UpdateInvitationsExpiration,
                                                               "Party invitations expiration updater");
            }
        }

        private class Invitation
        {
            public readonly ICharacter Invitee;

            public readonly ICharacter Inviter;

            public readonly ILogicObject ToParty;

            public Invitation(ILogicObject party, ICharacter inviter, ICharacter invitee)
            {
                this.ToParty = party;
                this.Inviter = inviter;
                this.Invitee = invitee;
                this.ResetExpirationDate();
            }

            public double ExpirationDate { get; private set; }

            public void ResetExpirationDate()
            {
                this.ExpirationDate = Api.Server.Game.FrameTime
                                      + PartyConstants.PartyInvitationLifetimeSeconds;
            }
        }
    }
}