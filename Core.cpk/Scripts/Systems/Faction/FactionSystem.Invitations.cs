namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    public partial class FactionSystem
    {
        /// <summary>
        /// Please do not modify this collection.
        /// </summary>
        public static readonly ObservableCollection<ClientInvitationEntry> ClientCurrentFactionCreatedInvitations
            = IsClient
                  ? new ObservableCollection<ClientInvitationEntry>()
                  : null;

        /// <summary>
        /// Please do not modify this collection.
        /// </summary>
        public static readonly ObservableCollection<ClientInvitationEntry> ClientCurrentReceivedInvitations
            = IsClient
                  ? new ObservableCollection<ClientInvitationEntry>()
                  : null;

        [RemoteEnum]
        public enum InvitationAcceptResult : byte
        {
            Unknown = 0,

            Success = 1,

            [Description(CoreStrings.Faction_FactionFull)]
            ErrorFactionFull = 2,

            [Description(CoreStrings.Faction_ErrorUnderJoinCooldown)]
            ErrorCooldownLeftRecently = 3
        }

        [RemoteEnum]
        public enum InvitationCreateResult : byte
        {
            Unknown = 0,

            Success = 1,

            [Description(CoreStrings.PlayerNotFound)]
            ErrorInviteeNotFound = 2,

            [Description(CoreStrings.Faction_FactionFull)]
            ErrorFactionFull = 3,

            [Description("Player is already a member of your faction.")]
            ErrorInviteeAlreadySameFactionMember = 4,

            [Description("Player is a member of another faction and cannot be invited.")]
            ErrorInviteeMemberAnotherFaction = 5
        }

        public static bool ClientCanInviteToFaction(string name)
        {
            if (ClientCurrentFaction is null
                || !ClientHasAccessRight(FactionMemberAccessRights.Recruitment))
            {
                // current player is not a faction officer
                return false;
            }

            if (name == Api.Client.Characters.CurrentPlayerCharacter?.Name)
            {
                // cannot invite self
                return false;
            }

            return !ClientIsFactionMember(name);
        }

        public static async void ClientInvitationAccept(string clanTag)
        {
            if (NewbieProtectionSystem.ClientIsNewbie)
            {
                NewbieProtectionSystem.ClientNotifyNewbieCannotPerformAction(null);
                return;
            }

            if (ClientCheckIsUnderJoinCooldown(showErrorNotification: true))
            {
                return;
            }

            var timeRemains = await Instance.CallServer(
                                  _ => _.ServerRemote_GetCooldownRemainsToJoinReturnToFaction(clanTag));
            if (timeRemains > 0)
            {
                var factionEmblem =
                    await ClientFactionEmblemTextureProvider.GetEmblemTextureAsync(clanTag, useCache: true);
                NotificationSystem.ClientShowNotification(
                    title: CoreStrings.Faction_ErrorUnderJoinCooldown,
                    // TODO: consider using a separate text constant here
                    message: string.Format(CoreStrings.ShieldProtection_CooldownRemains_Format,
                                           ClientTimeFormatHelper.FormatTimeDuration(timeRemains)),
                    NotificationColor.Bad,
                    icon: factionEmblem);
                return;
            }

            DialogWindow.ShowDialog(
                title: CoreStrings.Faction_Join,
                text: string.Format(CoreStrings.Faction_DialogJoinConfirmation_Message_Format,
                                    @$"\[{clanTag}\]")
                      + "[br]"
                      + "[br]"
                      + CoreStrings.Faction_JoinCooldown_Description
                      + " ("
                      + ClientDefaultJoinCooldownDurationText
                      + ")",
                okAction: async () =>
                          {
                              var result = await Instance.CallServer(
                                               _ => _.ServerRemote_InvitationAccept(clanTag));

                              if (result != InvitationAcceptResult.Success)
                              {
                                  NotificationSystem.ClientShowNotification(
                                      title: null,
                                      result.GetDescription(),
                                      NotificationColor.Bad);
                              }
                          },
                okText: CoreStrings.Faction_Join,
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        public static void ClientInvitationDecline(string clanTag)
        {
            Instance.CallServer(_ => _.ServerRemote_InvitationDecline(clanTag));
        }

        public static async void ClientOfficerInviteMember(string inviteeName)
        {
            if (ClientIsFactionMember(inviteeName))
            {
                ProcessResult(InvitationCreateResult.ErrorInviteeAlreadySameFactionMember);
                return;
            }

            ClientValidateHasAccessRights(FactionMemberAccessRights.Recruitment);

            ProcessResult(
                await Instance.CallServer(
                    _ => _.ServerRemote_OfficerInviteMember(inviteeName)));

            void ProcessResult(InvitationCreateResult result)
            {
                if (result == InvitationCreateResult.Success)
                {
                    //ClientFactionInvitationNotificationSystem.ShowNotificationInviteSent(inviteeName);
                    return;
                }

                NotificationSystem.ClientShowNotification(
                    title: null,
                    result.GetDescription(),
                    NotificationColor.Bad);
            }
        }

        public static void ClientOfficerRemoveAllInvitations()
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.Recruitment);
            Instance.CallServer(
                _ => _.ServerRemote_OfficerRemoveAllInvitations());
        }

        public static void ClientOfficerRemoveInvitation(string inviteeName)
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.Recruitment);
            Instance.CallServer(
                _ => _.ServerRemote_OfficerRemoveInvitation(inviteeName));
        }

        public static InvitationAcceptResult ServerInvitationAccept(ICharacter invitee, ILogicObject faction)
        {
            return ServerInvitations.Accept(invitee, faction);
        }

        public static InvitationCreateResult ServerInviteMember(ICharacter invitee, ICharacter inviter)
        {
            return ServerInvitations.AddInvitation(invitee, inviter);
        }

        private void ClientRemote_InvitationAddedOrUpdated(string clanTag, string inviterName, double expirationDate)
        {
            this.ClientRemote_InvitationRemoved(clanTag);

            var inviteeName = Client.Characters.CurrentPlayerCharacter.Name;
            ClientCurrentReceivedInvitations.Add(
                new ClientInvitationEntry(clanTag,
                                          inviterName,
                                          inviteeName,
                                          expirationDate));
        }

        private void ClientRemote_InvitationRemoved(string clanTag)
        {
            for (var index = 0; index < ClientCurrentReceivedInvitations.Count; index++)
            {
                var invitation = ClientCurrentReceivedInvitations[index];
                if (!string.Equals(invitation.ClanTag, clanTag, StringComparison.Ordinal))
                {
                    continue;
                }

                ClientCurrentReceivedInvitations.RemoveAt(index);
                break;
            }
        }

        private void ClientRemote_InvitationsList(
            List<(string clanTag, string inviterName, double expirationDate)> invitations)
        {
            foreach (var entry in ClientCurrentReceivedInvitations.ToList())
            {
                this.ClientRemote_InvitationRemoved(entry.ClanTag);
            }

            ClientCurrentReceivedInvitations.Clear();

            foreach (var entry in invitations)
            {
                this.ClientRemote_InvitationAddedOrUpdated(entry.clanTag,
                                                           entry.inviterName,
                                                           entry.expirationDate);
            }
        }

        private void ClientRemote_OfficerInvitationAddedOrUpdated(
            string inviterName,
            string inviteeName,
            double expirationDate)
        {
            this.ClientRemote_OfficerInvitationRemoved(inviteeName);

            var clanTag = SharedGetClanTag(ClientCurrentFaction);
            ClientCurrentFactionCreatedInvitations.Add(
                new ClientInvitationEntry(clanTag,
                                          inviterName,
                                          inviteeName,
                                          expirationDate));
        }

        private void ClientRemote_OfficerInvitationRemoved(string inviteeName)
        {
            for (var index = 0; index < ClientCurrentFactionCreatedInvitations.Count; index++)
            {
                var invitation = ClientCurrentFactionCreatedInvitations[index];
                if (!string.Equals(inviteeName, invitation.InviteeName, StringComparison.Ordinal))
                {
                    continue;
                }

                ClientCurrentFactionCreatedInvitations.RemoveAt(index);
                break;
            }
        }

        private void ClientRemote_OfficerInvitationsList(
            List<(string inviterName, string inviteeName, double expirationDate)> invitations)
        {
            foreach (var entry in ClientCurrentFactionCreatedInvitations.ToList())
            {
                this.ClientRemote_OfficerInvitationRemoved(entry.InviteeName);
            }

            ClientCurrentFactionCreatedInvitations.Clear();

            foreach (var entry in invitations)
            {
                this.ClientRemote_OfficerInvitationAddedOrUpdated(entry.inviterName,
                                                                  entry.inviteeName,
                                                                  entry.expirationDate);
            }
        }

        /// <summary>
        /// Gets how many seconds remains until player can join-return the specified faction.
        /// If player has never left that faction or the cooldown has expired, the result will be 0.
        /// Please note: it doesn't perform a check to detect the general cooldown
        /// and intended only for join-return case.
        /// </summary>
        private double ServerRemote_GetCooldownRemainsToJoinReturnToFaction(string clanTag)
        {
            var character = ServerRemoteContext.Character;
            var faction = ServerGetFactionByClanTag(clanTag);
            ServerInvitations.ServerIsUnderJoinCooldownForFaction(character, faction, out var cooldownTimeRemains);
            return cooldownTimeRemains;
        }

        private InvitationAcceptResult ServerRemote_InvitationAccept(string clanTag)
        {
            try
            {
                var invitee = ServerRemoteContext.Character;
                var faction = ServerGetFactionByClanTag(clanTag);
                return ServerInvitationAccept(invitee: invitee, faction);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return InvitationAcceptResult.Unknown;
            }
        }

        private void ServerRemote_InvitationDecline(string clanTag)
        {
            var faction = ServerGetFactionByClanTag(clanTag);
            ServerInvitations.Remove(ServerRemoteContext.Character, faction);
        }

        [RemoteCallSettings(timeInterval: 2)]
        private InvitationCreateResult ServerRemote_OfficerInviteMember(string inviteeName)
        {
            try
            {
                var officer = ServerRemoteContext.Character;
                ServerValidateHasAccessRights(officer,
                                              FactionMemberAccessRights.Recruitment);

                var invitee = Server.Characters.GetPlayerCharacter(inviteeName);
                if (invitee is null)
                {
                    return InvitationCreateResult.ErrorInviteeNotFound;
                }

                return ServerInviteMember(invitee, inviter: officer);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return InvitationCreateResult.Unknown;
            }
        }

        private void ServerRemote_OfficerRemoveAllInvitations()
        {
            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.Recruitment,
                                          out var faction);

            ServerInvitations.RemoveAllInvitationsInFaction(faction);
        }

        private void ServerRemote_OfficerRemoveInvitation(string inviteeName)
        {
            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.Recruitment,
                                          out var faction);

            var invitee = Server.Characters.GetPlayerCharacter(inviteeName);
            if (invitee is null)
            {
                Logger.Warning("Invitee not found: " + inviteeName);
                return;
            }

            ServerInvitations.Remove(invitee, faction);
        }

        public readonly struct ClientInvitationEntry
        {
            public readonly string ClanTag;

            public readonly double ExpirationDate;

            public readonly string InviteeName;

            public readonly string InviterName;

            public ClientInvitationEntry(
                string clanTag,
                string inviterName,
                string inviteeName,
                double expirationDate)
            {
                this.ClanTag = clanTag;
                this.InviteeName = inviteeName;
                this.InviterName = inviterName;
                this.ExpirationDate = expirationDate;
            }
        }

        private static class ServerInvitations
        {
            public static InvitationAcceptResult Accept(ICharacter invitee, ILogicObject faction)
            {
                if (ServerGetFaction(invitee) is { } currentInviteeFaction)
                {
                    // invitee already has a faction
                    // maybe the Accept request was received more than once?
                    return ReferenceEquals(faction, currentInviteeFaction)
                               // already a member of the same faction
                               ? InvitationAcceptResult.Success
                               : InvitationAcceptResult.Unknown;
                }

                if (NewbieProtectionSystem.SharedIsNewbie(invitee)
                    || SharedIsUnderJoinCooldown(invitee))
                {
                    // should be impossible as client performs the same check
                    return InvitationAcceptResult.Unknown;
                }

                var invitation = GetInvitation(faction, invitee);
                if (invitation is null)
                {
                    // should never happen
                    return InvitationAcceptResult.Unknown;
                }

                var inviterFactionMembers = ServerGetFactionMembersReadOnly(faction);
                var maxMembers = FactionConstants.SharedGetFactionMembersMax(
                    Faction.GetPublicState(faction).Kind);

                if (inviterFactionMembers.Count >= maxMembers)
                {
                    return InvitationAcceptResult.ErrorFactionFull;
                }

                if (ServerIsUnderJoinCooldownForFaction(invitee, faction, out _))
                {
                    return InvitationAcceptResult.ErrorCooldownLeftRecently;
                }

                ServerApplications.RemoveAllApplicationsByApplicant(invitee);
                RemoveAllInvitationsFor(invitee);

                ServerAddMember(invitee, faction, FactionMemberRole.Member);
                ServerAddLogEntry(faction,
                                  new FactionLogEntryMemberJoined(member: invitee,
                                                                  approvedByOfficer: invitation.Inviter));

                Logger.Important($"{invitee} joined {faction} by invitation from {invitation.Inviter}");
                return InvitationAcceptResult.Success;
            }

            public static InvitationCreateResult AddInvitation(ICharacter invitee, ICharacter inviter)
            {
                var faction = ServerGetFaction(inviter);
                if (faction is null)
                {
                    // should be impossible
                    return InvitationCreateResult.Unknown;
                }

                var inviteeFaction = ServerGetFaction(invitee);
                if (inviteeFaction is not null)
                {
                    return ReferenceEquals(faction, inviteeFaction)
                               ? InvitationCreateResult.ErrorInviteeAlreadySameFactionMember
                               : InvitationCreateResult.ErrorInviteeMemberAnotherFaction;
                }

                var invitation = GetInvitation(faction, invitee);
                if (invitation is not null)
                {
                    if (invitation.Inviter.Name != inviter.Name)
                    {
                        // remove and create a new invitation from a different inviter
                        RemoveInvitation(invitation, faction);
                    }
                    else
                    {
                        // extend the existing invitation
                        invitation.ResetExpirationDate();
                        NotifyInvitationAddedOrUpdated(faction, invitation);
                        return InvitationCreateResult.Success;
                    }
                }

                var factionMemberEntries = ServerGetFactionMembersReadOnly(faction);
                var maxMembers = FactionConstants.SharedGetFactionMembersMax(
                    Faction.GetPublicState(faction).Kind);

                if (factionMemberEntries.Count >= maxMembers)
                {
                    return InvitationCreateResult.ErrorFactionFull;
                }

                if (ServerApplicationAccept(officer: inviter, applicant: invitee)
                    == ApplicationAcceptResult.Success)
                {
                    // there was already an application from the invitee and it was accepted now
                    // no need to send an invite
                    return InvitationCreateResult.Success;
                }

                invitation = new FactionInvitation(inviter, invitee);
                GetInvitations(faction).Add(invitation);

                Logger.Important($"{invitee} has been invited by {inviter} to {faction}");

                NotifyInvitationAddedOrUpdated(faction, invitation);
                return InvitationCreateResult.Success;
            }

            public static List<FactionInvitation> GetInvitations(ILogicObject faction)
            {
                return Faction.GetPrivateState(faction).ServerInvitationsList;
            }

            public static void OfficerSyncInvitationsList(ICharacter officer, ILogicObject faction)
            {
                var factionInvitations = GetInvitations(faction);
                if (factionInvitations.Count == 0)
                {
                    return;
                }

                var invitations = new List<(string inviterName, string inviteeName, double expirationDate)>();
                foreach (var invitation in factionInvitations)
                {
                    invitations.Add(
                        (invitation.Inviter.Name,
                         invitation.Invitee.Name,
                         invitation.ExpirationDate));
                }

                Instance.CallClient(officer,
                                    _ => _.ClientRemote_OfficerInvitationsList(invitations));
            }

            public static void Remove(ICharacter invitee, ILogicObject faction)
            {
                var invitation = GetInvitation(faction, invitee);
                if (invitation is not null)
                {
                    RemoveInvitation(invitation, faction);
                }
            }

            public static void RemoveAllInvitationsBy(ICharacter inviter, ILogicObject faction)
            {
                var invitationsList = GetInvitations(faction);
                for (var index = 0; index < invitationsList.Count; index++)
                {
                    var invitation = invitationsList[index];
                    if (invitation.Inviter != inviter)
                    {
                        continue;
                    }

                    invitationsList.RemoveAt(index);
                    index--;

                    NotifyInvitationRemoved(invitation, faction);
                }
            }

            public static void RemoveAllInvitationsFor(ICharacter invitee)
            {
                foreach (var faction in ServerGetFactionsTempList().EnumerateAndDispose())
                {
                    var invitationsList = GetInvitations(faction);
                    for (var index = 0; index < invitationsList.Count; index++)
                    {
                        var invitation = invitationsList[index];
                        if (invitation.Invitee != invitee)
                        {
                            continue;
                        }

                        invitationsList.RemoveAt(index);
                        index--;

                        NotifyInvitationRemoved(invitation, faction);
                    }
                }
            }

            public static void RemoveAllInvitationsInFaction(ILogicObject faction)
            {
                var invitations = GetInvitations(faction);
                try
                {
                    foreach (var invitation in invitations)
                    {
                        NotifyInvitationRemoved(invitation, faction);
                    }
                }
                finally
                {
                    invitations.Clear();
                }
            }

            public static bool ServerIsUnderJoinCooldownForFaction(
                ICharacter character,
                ILogicObject faction,
                out double cooldownTimeRemains)
            {
                if (!Faction.GetPrivateState(faction).ServerPlayerLeaveDateDictionary
                            .TryGetValue(character.Id, out var lastLeaveData))
                {
                    cooldownTimeRemains = 0;
                    return false;
                }

                cooldownTimeRemains = (lastLeaveData + FactionConstants.SharedFactionJoinReturnBackCooldownDuration)
                                      - Server.Game.FrameTime;
                if (cooldownTimeRemains < 0)
                {
                    cooldownTimeRemains = 0;
                }

                return cooldownTimeRemains > 0;
            }

            public static void SyncAllReceivedInvitationsList(ICharacter invitee)
            {
                using var tempListInvitations =
                    Api.Shared.GetTempList<(string clanTag, string inviterName, double expirationDate)>();

                foreach (var faction in ServerGetFactionsTempList().EnumerateAndDispose())
                {
                    foreach (var invitation in GetInvitations(faction))
                    {
                        if (!ReferenceEquals(invitation.Invitee, invitee))
                        {
                            continue;
                        }

                        tempListInvitations.Add((SharedGetClanTag(faction),
                                                 invitation.Inviter.Name,
                                                 invitation.ExpirationDate));
                    }
                }

                if (tempListInvitations.Count == 0)
                {
                    return;
                }

                // send a copy of the list
                var invitationsList = tempListInvitations.AsList().ToList();
                Instance.CallClient(invitee,
                                    _ => _.ClientRemote_InvitationsList(invitationsList));
            }

            private static FactionInvitation GetInvitation(ILogicObject faction, ICharacter invitee)
            {
                foreach (var invitation in GetInvitations(faction))
                {
                    if (ReferenceEquals(invitee, invitation.Invitee))
                    {
                        return invitation;
                    }
                }

                return null;
            }

            private static void NotifyInvitationAddedOrUpdated(ILogicObject faction, FactionInvitation invitation)
            {
                var clanTag = SharedGetClanTag(faction);
                Instance.CallClient(invitation.Invitee,
                                    _ => _.ClientRemote_InvitationAddedOrUpdated(clanTag,
                                        invitation.Inviter.Name,
                                        invitation.ExpirationDate));

                Instance.CallClient(
                    ServerEnumerateFactionOfficers(faction, FactionMemberAccessRights.Recruitment),
                    _ => _.ClientRemote_OfficerInvitationAddedOrUpdated(invitation.Inviter.Name,
                                                                        invitation.Invitee.Name,
                                                                        invitation.ExpirationDate));
            }

            private static void NotifyInvitationRemoved(FactionInvitation invitation, ILogicObject faction)
            {
                var clanTag = SharedGetClanTag(faction);
                Instance.CallClient(invitation.Invitee,
                                    _ => _.ClientRemote_InvitationRemoved(clanTag));

                Instance.CallClient(
                    ServerEnumerateFactionOfficers(faction, FactionMemberAccessRights.Recruitment),
                    _ => _.ClientRemote_OfficerInvitationRemoved(invitation.Invitee.Name));
            }

            private static void RemoveInvitation(FactionInvitation invitation, ILogicObject faction)
            {
                Api.Assert(invitation is not null, "Invitation cannot be null");
                GetInvitations(faction).Remove(invitation);
                NotifyInvitationRemoved(invitation, faction);
            }

            private static void UpdateInvitationsExpiration()
            {
                // perform update once per minute per faction
                const double spreadDeltaTime = 60;
                var currentTime = Server.Game.FrameTime;

                using var tempListFactions = Api.Shared.GetTempList<ILogicObject>();

                ProtoFaction.EnumerateGameObjectsWithSpread(tempListFactions.AsList(),
                                                            spreadDeltaTime: spreadDeltaTime,
                                                            Server.Game.FrameNumber,
                                                            Server.Game.FrameRate);

                foreach (var faction in tempListFactions.AsList())
                {
                    var invitationsList = GetInvitations(faction);
                    for (var index = 0; index < invitationsList.Count; index++)
                    {
                        var invitation = invitationsList[index];
                        if (invitation.ExpirationDate > currentTime)
                        {
                            // Not expired.
                            // Please note we don't return here.
                            // It could be a nice optimization but not applicable here as
                            // the invitation expiration date could be extended.
                            continue;
                        }

                        // expired
                        invitationsList.RemoveAt(index);
                        index--;

                        NotifyInvitationRemoved(invitation, faction);
                    }
                }
            }

            private class Bootstrapper : BaseBootstrapper
            {
                public override void ServerInitialize(IServerConfiguration serverConfiguration)
                {
                    TriggerEveryFrame.ServerRegister(UpdateInvitationsExpiration,
                                                     $"{nameof(FactionSystem)}.{nameof(UpdateInvitationsExpiration)}");
                }
            }
        }

        [Serializable]
        public class FactionInvitation
        {
            public readonly ICharacter Invitee;

            public readonly ICharacter Inviter;

            public FactionInvitation(ICharacter inviter, ICharacter invitee)
            {
                this.Inviter = inviter;
                this.Invitee = invitee;
                this.ResetExpirationDate();
            }

            public double ExpirationDate { get; private set; }

            public void ResetExpirationDate()
            {
                this.ExpirationDate = Api.Server.Game.FrameTime
                                      + FactionConstants.FactionInvitationLifetimeSeconds;
            }
        }
    }
}