namespace AtomicTorch.CBND.CoreMod.Systems.Party
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
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
            = "{0} has removed you from the party";

        public static readonly ObservableCollection<string> ClientCurrentInvitationsFromCharacters
            = IsClient
                  ? new ObservableCollection<string>()
                  : null;

        private static readonly Dictionary<ICharacter, ILogicObject> ServerCharacterPartyDictionary
            = IsServer
                  ? new Dictionary<ICharacter, ILogicObject>()
                  : null;

        private static NetworkSyncList<string> clientCurrentPartyMembersList;

        public static event Action ClientCurrentPartyChanged;

        public static event Action<(string name, bool isAdded)> ClientCurrentPartyMemberAddedOrRemoved;

        public static event Action<ICharacter> ServerCharacterJoinedOrLeftParty;

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
            ErrorMuted = 5
        }

        public static ILogicObject ClientCurrentParty { get; private set; }

        [NotLocalizable]
        public override string Name => "Party system";

        public static void ClientCreateParty()
        {
            Api.Assert(ClientCurrentParty == null, "Already have a party");
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
            Api.Assert(ClientCurrentParty != null, "Don't have a party");
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
                if (result != InvitationCreateResult.Success)
                {
                    NotificationSystem.ClientShowNotification(
                        title: null,
                        result.GetDescription(),
                        NotificationColor.Bad);
                }
            }
        }

        public static bool ClientIsPartyMember(string name)
        {
            return clientCurrentPartyMembersList?.Contains(name) ?? false;
        }

        public static void ClientLeaveParty()
        {
            Api.Assert(ClientCurrentParty != null, "Don't have a party");
            Instance.CallServer(_ => _.ServerRemote_LeaveParty());
        }

        public static void ClientRemovePartyMember(string memberName)
        {
            Api.Assert(ClientCurrentParty != null, "Don't have a party");
            Api.Assert(clientCurrentPartyMembersList[0] == ClientCurrentCharacterHelper.Character.Name,
                       "You're not the party owner");
            Instance.CallServer(_ => _.ServerRemote_RemovePartyMember(memberName));
        }

        public static void ServerCreateParty(ICharacter character)
        {
            Api.Assert(!character.IsNpc, "NPC cannot create a party");

            var party = ServerGetParty(character);
            if (party != null)
            {
                throw new Exception($"Player already has a party: {character} in {party}");
            }

            party = Server.World.CreateLogicObject<Party>();
            Logger.Important($"Created party: {party} for {character}", character);
            ServerAddMember(character, party);
        }

        public static ILogicObject ServerGetParty(ICharacter character)
        {
            return ServerCharacterPartyDictionary.Find(character);
        }

        public static ILogicObject ServerGetPartyChat(ILogicObject party)
        {
            return party.GetPrivateState<Party.PartyPrivateState>()
                        .ServerPartyChatHolder;
        }

        public static IReadOnlyList<string> ServerGetPartyMembersReadOnly(ICharacter character)
        {
            var party = ServerGetParty(character);
            return party != null
                       ? ServerGetPartyMembersReadOnly(party)
                       : Array.Empty<string>();
        }

        public static IReadOnlyList<string> ServerGetPartyMembersReadOnly(ILogicObject party)
        {
            return party != null
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

        public static void ServerLeaveParty(ICharacter character)
        {
            var party = ServerGetParty(character);
            if (party == null)
            {
                throw new Exception("Not a member of any party");
            }

            ServerRemoveMember(character.Name, party);
            ChatSystem.ServerSendServiceMessage(
                ServerGetPartyChat(party),
                string.Format(Notification_PlayerLeftPartyFormat, character.Name));
        }

        internal static void ServerRegisterParty(ILogicObject party)
        {
            var members = ServerGetPartyMembersReadOnly(party);

            foreach (var memberName in members)
            {
                var character = Server.Characters.GetPlayerCharacter(memberName);
                if (character == null)
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
            if (clientCurrentPartyMembersList != null)
            {
                clientCurrentPartyMembersList.ClientElementInserted -= ClientCurrentPartyMemberInsertedHandler;
                clientCurrentPartyMembersList.ClientElementRemoved -= ClientCurrentPartyMemberRemovedHandler;
                clientCurrentPartyMembersList = null;
            }

            if (handler != null
                && previousPartyMembersList != null)
            {
                foreach (var member in previousPartyMembersList)
                {
                    Api.SafeInvoke(() => handler((member, isAdded: false)));
                }
            }

            ClientCurrentParty = party;
            clientCurrentPartyMembersList = party?.GetPrivateState<Party.PartyPrivateState>()
                                                 .Members;

            if (clientCurrentPartyMembersList != null)
            {
                clientCurrentPartyMembersList.ClientElementInserted += ClientCurrentPartyMemberInsertedHandler;
                clientCurrentPartyMembersList.ClientElementRemoved += ClientCurrentPartyMemberRemovedHandler;
            }

            Api.SafeInvoke(() => ClientCurrentPartyChanged?.Invoke());

            if (handler != null
                && clientCurrentPartyMembersList != null)
            {
                foreach (var member in clientCurrentPartyMembersList)
                {
                    Api.SafeInvoke(() => handler((member, isAdded: true)));
                }
            }
        }

        private static void RemoveParty(ILogicObject party)
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
            Server.World.DestroyObject(ServerGetPartyChat(party));
            Server.World.DestroyObject(party);
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

            if (currentParty != null)
            {
                throw new Exception($"Player already has a party: {character} in {party}");
            }

            var members = ServerGetPartyMembersEditable(party);
            if (members.Count + 1 >= PartyConstants.PartyMembersMax)
            {
                throw new Exception("Party size exceeded - max " + PartyConstants.PartyMembersMax);
            }

            members.Add(character.Name);
            ServerCharacterPartyDictionary[character] = party;
            Logger.Important($"Player joined party: {character} in {party}", character);

            ServerInvitations.RemoveAllInvitationsFor(character);

            Api.SafeInvoke(
                () => ServerCharacterJoinedOrLeftParty?.Invoke(character));

            // add this with some delay to prevent from the bug when the player name listed twice due to the late delta-replication
            ServerTimersSystem.AddAction(delaySeconds: 0.1,
                                         () => ServerSendCurrentParty(character));
        }

        private static NetworkSyncList<string> ServerGetPartyMembersEditable(ILogicObject party)
        {
            return party.GetPrivateState<Party.PartyPrivateState>().Members;
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
                if (character != null)
                {
                    if (ServerCharacterPartyDictionary.TryGetValue(character, out var currentParty)
                        && currentParty == party)
                    {
                        ServerCharacterPartyDictionary.Remove(character);
                    }

                    // remove all requests by this character
                    ServerInvitations.RemoveAllInvitationsBy(character);

                    Api.SafeInvoke(
                        () => ServerCharacterJoinedOrLeftParty?.Invoke(character));

                    Server.World.ForceExitScope(character, party);
                    // send "no party" for this player
                    ServerSendCurrentParty(character);
                    ChatSystem.ServerRemoveChatRoomFromPlayerScope(character, ServerGetPartyChat(party));
                }

                if (members.Count == 0)
                {
                    RemoveParty(party);
                }

                return;
            }

            Logger.Warning($"Party member is not found: {characterName} in {party}");
        }

        private static void ServerSendCurrentParty(ICharacter character)
        {
            var party = ServerGetParty(character);
            if (party == null)
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
                message: string.Format(Notification_RemovedFromTheParty, ownerName));
        }

        private void ServerRemote_CreateParty()
        {
            ServerCreateParty(ServerRemoteContext.Character);
        }

        private InvitationAcceptResult ServerRemote_InvitationAccept(string inviterName)
        {
            try
            {
                var inviter = Server.Characters.GetPlayerCharacter(inviterName);
                if (inviter == null)
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
            if (inviter == null)
            {
                throw new Exception("Inviter player not found: " + inviterName);
            }

            ServerInvitationDecline(invitee: ServerRemoteContext.Character, inviter);
        }

        private InvitationCreateResult ServerRemote_InviteMember(string inviteeName)
        {
            try
            {
                var invitee = Server.Characters.GetPlayerCharacter(inviteeName);
                if (invitee == null)
                {
                    return InvitationCreateResult.ErrorInviteeNotFound;
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
            if (party == null)
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
            ChatSystem.ServerSendServiceMessage(
                ServerGetPartyChat(party),
                string.Format(Notification_PlayerRemovedPartyMemberFormat, memberName, ownerName));

            var member = Server.Characters.GetPlayerCharacter(memberName);
            this.CallClient(member,
                            _ => _.ClientRemote_RemovedFromParty(ownerName));
        }

        private void ServerRemote_RequestCurrentPartyAndInvitations()
        {
            var character = ServerRemoteContext.Character;
            ServerSendCurrentParty(character);

            ServerInvitations.ResendAllInvitations(invitee: character);
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

                    if (Api.Client.Characters.CurrentPlayerCharacter != null)
                    {
                        Instance.CallServer(_ => _.ServerRemote_RequestCurrentPartyAndInvitations());
                    }
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                // doesn't work here as FindGameObjectsOfProto is not available during bootstrap
                //var allParties = Server.World.FindGameObjectsOfProto<ILogicObject, Party>();
                //foreach (var party in allParties)
                //{
                //    ServerRegisterParty(party);
                //}

                TriggerTimeInterval.ServerConfigureAndRegister(TimeSpan.FromSeconds(1),
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
                this.ExpirationDate = Api.Server.Game.FrameTime + PartyConstants.PartyInvitationLifetimeSeconds;
            }
        }

        private class ServerInvitations
        {
            private static readonly List<Invitation> InvitationsList
                = IsServer
                      ? new List<Invitation>()
                      : null;

            public static InvitationAcceptResult Accept(ICharacter invitee, ICharacter inviter)
            {
                var inviterParty = ServerGetParty(inviter);
                var invitation = GetInvitation(inviterParty, invitee);
                if (invitation == null)
                {
                    return InvitationAcceptResult.ErrorNoInvitationFoundOrExpired;
                }

                // should never happen
                Api.Assert(inviterParty != null, "Inviter must have a party");
                var currentInviteeParty = ServerGetParty(invitee);
                Api.Assert(inviterParty != currentInviteeParty, "Cannot join the same party");

                var inviterPartyMembers = ServerGetPartyMembersReadOnly(inviterParty);
                if (inviterPartyMembers.Count + 1 >= PartyConstants.PartyMembersMax)
                {
                    return InvitationAcceptResult.ErrorPartyFull;
                }

                if (currentInviteeParty != null)
                {
                    // invitee already has a party - leave it first
                    ServerLeaveParty(invitee);
                }

                RemoveInvitation(invitation, notifyInvitee: false);
                RemoveAllInvitationsFor(invitee);

                ChatSystem.ServerSendServiceMessage(
                    ServerGetPartyChat(inviterParty),
                    string.Format(Notification_PlayerAcceptedInviteFormat, invitee.Name, inviter.Name));

                ServerAddMember(invitee, inviterParty);

                return InvitationAcceptResult.Success;
            }

            public static InvitationCreateResult AddInvitation(ICharacter invitee, ICharacter inviter)
            {
                var party = ServerGetParty(inviter);
                if (party == null)
                {
                    Logger.Warning("Player don't have a party and so cannot invite anyone", inviter);
                    return InvitationCreateResult.Unknown;
                }

                if (ServerPlayerMuteSystem.IsMuted(invitee.Name, out _))
                {
                    return InvitationCreateResult.ErrorMuted;
                }

                var invitation = GetInvitation(party, invitee);
                if (invitation != null)
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
                if (partyMembers.Count + 1 >= PartyConstants.PartyMembersMax)
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
                    string.Format(Notification_PlayerInvitedFormat, inviter.Name, invitee.Name));

                return InvitationCreateResult.Success;
            }

            public static void Decline(ICharacter invitee, ICharacter inviter)
            {
                var party = ServerGetParty(inviter);
                if (party == null)
                {
                    throw new Exception("Player don't have a party");
                }

                var invitation = GetInvitation(party, invitee);
                if (invitation != null)
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
                Api.Assert(invitation != null, "Invitation cannot be null");
                InvitationsList.Remove(invitation);

                if (notifyInvitee)
                {
                    NotifyInvitationRemoved(invitation);
                }
            }
        }
    }
}