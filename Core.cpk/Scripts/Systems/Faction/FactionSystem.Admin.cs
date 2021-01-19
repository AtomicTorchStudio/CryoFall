namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.ProfanityFiltering;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using JetBrains.Annotations;

    public partial class FactionSystem
    {
        public const ushort MaxDescriptionLengthPrivate = 1024;

        public const ushort MaxDescriptionLengthPublic = 256;

        public static void ClientLeaderDissolveFaction()
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.Leader);

            if (LandClaimSystem.ClientEnumerateAllCurrentFactionAreas()
                               .Any())
            {
                NotificationSystem.ClientShowNotification(
                    title: null,
                    CoreStrings.Faction_DialogDissolveFaction_ErrorHasLandClaimsClaims,
                    NotificationColor.Bad);
                return;
            }

            Instance.CallServer(
                _ => _.ServerRemote_LeaderDissolveFaction());
            Logger.Important("Requested faction dissolve");
        }

        public static void ClientLeaderSetMemberRoleAccessRight(
            FactionMemberRole role,
            FactionMemberAccessRights accessRights,
            bool isAssigned)
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.Leader);
            var currentAccessRights = SharedGetRoleAccessRights(ClientCurrentFaction, role);
            FactionMemberAccessRights newAccessRights;

            if (isAssigned)
            {
                if (currentAccessRights.HasFlag(accessRights))
                {
                    return;
                }

                newAccessRights = currentAccessRights | accessRights;
            }
            else // remove
            {
                if (!currentAccessRights.HasFlag(accessRights))
                {
                    return;
                }

                newAccessRights = currentAccessRights & ~accessRights;
            }

            Instance.CallServer(
                _ => _.ServerRemote_LeaderSetRoleAccessRights(role, newAccessRights));
            Logger.Important(string.Format("Changed member role access right: {0} {1} {2}{3}New rights: {4}",
                                           role,
                                           isAssigned ? "assigned" : "removed",
                                           accessRights,
                                           Environment.NewLine,
                                           newAccessRights));
        }

        public static void ClientLeaderSetOfficerRoleTitle(
            FactionMemberRole role,
            FactionOfficerRoleTitle roleTitle)
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.Leader);
            SharedValidateIsOfficerRole(role);

            var binding = Faction.GetPrivateState(ClientCurrentFaction).OfficerRoleTitleBinding;
            if (binding.TryGetValue(role, out var currentRoleTitle)
                && currentRoleTitle == roleTitle)
            {
                return;
            }

            Instance.CallServer(
                _ => _.ServerRemote_LeaderSetOfficerRoleTitle(role, roleTitle));
            Logger.Important($"Officer role title changed: {role} -> {roleTitle}");
        }

        public static void ClientLeaderTransferOwnership(string memberName)
        {
            if (!ClientIsFactionMember(memberName))
            {
                throw new Exception("Faction member is not found: " + memberName);
            }

            if (memberName == ClientCurrentCharacterHelper.Character.Name)
            {
                return;
            }

            ClientValidateHasAccessRights(FactionMemberAccessRights.Leader);
            Instance.CallServer(
                _ => _.ServerRemote_LeaderTransferOwnership(memberName));
            Logger.Important("Requested faction ownership transfer to " + memberName);
        }

        public static void ClientOfficerRemoveFactionMember(string memberName, bool askConfirmation = true)
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.RemoveMembers);

            if (!askConfirmation)
            {
                CallServer();
                return;
            }

            var textBlock = new FormattedTextBlock()
            {
                Content = string.Format(CoreStrings.Faction_Dialog_RemoveFactionMemberConfirmation_MessageFormat,
                                        memberName),
                FontWeight = FontWeights.Bold,
                Foreground = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed6")
            };

            DialogWindow.ShowDialog(
                title: CoreStrings.Faction_RemoveMember,
                content: textBlock,
                okText: CoreStrings.Yes,
                okAction: CallServer,
                cancelText: CoreStrings.Button_Cancel,
                cancelAction: () => { },
                focusOnCancelButton: true);

            void CallServer()
            {
                Instance.CallServer(
                    _ => _.ServerRemote_OfficerRemoveFactionMember(memberName));
            }
        }

        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public static Task<bool> ClientOfficerSetEmblem(FactionEmblem emblem)
        {
            throw new Exception("This feature is disabled");
#pragma warning disable 162
            ClientValidateHasAccessRights(FactionMemberAccessRights.EditDescription);

            if (emblem.Equals(
                Faction.GetPublicState(ClientCurrentFaction).Emblem))
            {
                // this faction already has the same emblem 
                return Task.FromResult(true);
            }

            return Instance.CallServer(
                _ => _.ServerRemote_OfficerSetEmblem(emblem));
#pragma warning restore 162
        }

        public static void ClientOfficerSetFactionDescription(string text, bool isPrivateDescription)
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.EditDescription);
            text = ProfanityFilteringSystem.SharedApplyFilters(text);
            SharedValidateDescriptionLength(text, isPrivateDescription);
            Instance.CallServer(
                _ => _.ServerRemote_OfficerSetDescription(text, isPrivateDescription));
        }

        public static void ClientOfficerSetMemberRole(string memberName, FactionMemberRole role)
        {
            if (!ClientIsFactionMember(memberName))
            {
                throw new Exception("Faction member is not found: " + memberName);
            }

            if (memberName == ClientCurrentCharacterHelper.Character.Name)
            {
                // cannot change self role
                return;
            }

            ClientValidateHasAccessRights(FactionMemberAccessRights.SetMemberRole);
            Instance.CallServer(
                _ => _.ServerRemote_OfficerSetMemberRole(memberName, role));
        }

        public static void ServerRemoveMemberFromCurrentFaction(ICharacter character)
        {
            if (character.IsNpc)
            {
                return;
            }

            var faction = ServerGetFaction(character);
            if (faction is null)
            {
                return;
            }

            if (ServerGetRole(character) == FactionMemberRole.Leader)
            {
                throw new Exception("Cannot remove a faction leader from the faction");
            }

            ServerRemoveMemberNoChecks(character.Name, faction);
            ServerAddLogEntry(faction,
                              new FactionLogEntryMemberRemoved(character,
                                                               byOfficer: null));
        }

        /// <summary>
        /// Important: the validity of this action must be checked first
        /// as this method will only change (only) the role.
        /// (E.g. if checks are not made it's possible there will be more than a single leader
        /// or an officer may strip the leader of its role)
        /// </summary>
        public static void ServerSetMemberRoleNoChecks(
            string memberName,
            ILogicObject faction,
            FactionMemberRole role)
        {
            if (!SharedIsValidRole(role))
            {
                throw new Exception("Invalid role: " + role);
            }

            var members = ServerGetFactionMembersEditable(faction);

            for (var index = 0; index < members.Count; index++)
            {
                var entry = members[index];
                if (!string.Equals(entry.Name, memberName, StringComparison.Ordinal))
                {
                    continue;
                }

                // member found
                if (entry.Role == role)
                {
                    // already has the role
                    continue;
                }

                members[index] = new FactionMemberEntry(memberName, role);
                Logger.Important(
                    string.Format("Faction member role changed: {0} in {1} - role: {2}",
                                  memberName,
                                  faction,
                                  role));

                ServerOnFactionMemberAccessRightsChanged(faction,
                                                         memberName,
                                                         SharedGetRoleAccessRights(faction, role));
                return;
            }

            Logger.Warning($"Faction member is not found: {memberName} in {faction}");
        }

        public static void ServerTransferFactionOwnership(ICharacter currentLeader, string newLeaderName)
        {
            ServerValidateHasAccessRights(currentLeader,
                                          FactionMemberAccessRights.Leader,
                                          out var faction);

            if (SharedGetMemberEntry(newLeaderName, faction) is null)
            {
                throw new Exception($"Faction member is not found: {newLeaderName} in {faction}");
            }

            var newLeader = Server.Characters.GetPlayerCharacter(newLeaderName)
                            ?? throw new Exception("Character is not found: " + newLeaderName);

            if (ReferenceEquals(currentLeader, newLeader))
            {
                return;
            }

            ServerSetMemberRoleNoChecks(currentLeader.Name, faction, FactionMemberRole.Member);
            ServerSetMemberRoleNoChecks(newLeaderName,      faction, FactionMemberRole.Leader);
            Faction.GetPublicState(faction).LeaderName = newLeaderName;

            ServerAddLogEntry(faction,
                              new FactionLogEntryLeaderTransferredOwnership(fromLeader: currentLeader,
                                                                            toMember: newLeader));

            Logger.Important($"Player transferred faction ownership: {currentLeader} to {newLeader}",
                             currentLeader);
        }

        public static bool SharedIsValidClanTag(string text)
        {
            var originalClanTag = text;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (text.Length > 4)
            {
                return false;
            }

            text = text.ToUpperInvariant();

            // must start with a valid letter and the rest should be a valid letter or a digit 
            for (var index = 0; index < text.Length; index++)
            {
                var c = text[index];
                if (c >= 'A' && c <= 'Z'
                    || index > 0 && char.IsDigit(c))
                {
                    continue;
                }

                return false;
            }

            return !ClanTagFilterHelper.ContainsProfanityOrProhibited(originalClanTag);
        }

        private static void ServerAddMember(
            ICharacter character,
            ILogicObject faction,
            FactionMemberRole role)
        {
            Api.Assert(!character.IsNpc, "NPC cannot join a faction");

            if (!SharedIsValidRole(role))
            {
                throw new Exception("Invalid role: " + role);
            }

            var currentFaction = ServerGetFaction(character);
            if (currentFaction == faction)
            {
                // already in faction
                return;
            }

            if (currentFaction is not null)
            {
                throw new Exception($"Player already has a faction: {character} in {faction}");
            }

            // faction members cannot have a newbie protection
            NewbieProtectionSystem.ServerDisableNewbieProtection(character);

            var members = ServerGetFactionMembersEditable(faction);
            var factionPublicState = Faction.GetPublicState(faction);
            var maxMembers = FactionConstants.GetFactionMembersMax(factionPublicState.Kind);

            if (members.Count >= maxMembers)
            {
                throw new Exception("Faction size exceeded - max " + maxMembers);
            }

            if (role == FactionMemberRole.Leader)
            {
                foreach (var otherMember in members)
                {
                    if (otherMember.Role == FactionMemberRole.Leader)
                    {
                        throw new Exception("Faction can have only a single leader");
                    }
                }
            }

            members.Add(new FactionMemberEntry(character.Name, role));
            ServerCharacterFactionDictionary[character] = faction;
            PlayerCharacter.GetPublicState(character).ClanTag = factionPublicState.ClanTag;
            factionPublicState.PlayersNumberCurrent++;

            Logger.Important($"Player joined faction: {character} in {faction} - role: {role}",
                             character);

            ServerInvitations.RemoveAllInvitationsFor(character);

            Api.SafeInvoke(
                () => ServerCharacterJoinedOrLeftFaction?.Invoke(character,
                                                                 faction,
                                                                 isJoined: true));

            // add this with some delay to prevent from the bug when the player name listed twice due to the late delta-replication
            ServerTimersSystem.AddAction(delaySeconds: 0.1,
                                         () => ServerSendCurrentFaction(character));
        }

        private static void ServerOnFactionMemberAccessRightsChanged(
            ILogicObject faction,
            string memberName,
            FactionMemberAccessRights accessRights)
        {
            var member = Server.Characters.GetPlayerCharacter(memberName);
            if (member is null
                || !member.ServerIsOnline)
            {
                return;
            }

            // ensure the server will re-sync the faction-related data
            // after changing the character's access rights
            ServerPlayerOnlineStateChangedHandler(member, isOnline: false);
            ServerPlayerOnlineStateChangedHandler(member, isOnline: true);

            Api.SafeInvoke(
                () => ServerFactionMemberAccessRightsChanged?.Invoke(faction,
                                                                     memberName,
                                                                     accessRights));
        }

        private static void ServerRemoveMemberNoChecks(string characterName, ILogicObject faction)
        {
            var members = ServerGetFactionMembersEditable(faction);
            for (var index = 0; index < members.Count; index++)
            {
                var entry = members[index];
                if (!string.Equals(entry.Name, characterName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (entry.Role == FactionMemberRole.Leader)
                {
                }

                // member found
                members.RemoveAt(index);
                Faction.GetPublicState(faction).PlayersNumberCurrent--;
                Logger.Important($"Faction member removed: {characterName} from {faction}");

                var character = Server.Characters.GetPlayerCharacter(characterName);
                if (character is not null)
                {
                    // record the time when player has left or was removed from the faction
                    Faction.GetPrivateState(faction).ServerPlayerLeaveDateDictionary[character.Id]
                        = Server.Game.FrameTime;

                    ServerCharacterFactionDictionary.Remove(character);

                    PlayerCharacter.GetPublicState(character).ClanTag = null;
                    PlayerCharacter.GetPrivateState(character).LastFactionLeaveTime = Server.Game.FrameTime;

                    // remove all requests by this character
                    ServerInvitations.RemoveAllInvitationsBy(character, faction);

                    Api.SafeInvoke(
                        () => ServerCharacterJoinedOrLeftFaction?.Invoke(character,
                                                                         faction,
                                                                         isJoined: false));

                    Api.SafeInvoke(
                        () => ServerFactionMemberAccessRightsChanged?.Invoke(faction,
                                                                             characterName,
                                                                             FactionMemberAccessRights.None));

                    Server.World.ForceExitScope(character, faction);
                    // send "no faction" for this player
                    ServerSendCurrentFaction(character);
                    ChatSystem.ServerRemoveChatRoomFromPlayerScope(character, ServerGetFactionChat(faction));
                }

                if (members.Count == 0)
                {
                    ServerRemoveFaction(faction);
                }

                return;
            }

            Logger.Warning($"Faction member is not found: {characterName} in {faction}");
        }

        private static void ServerValidateRoleAccessRights(FactionMemberAccessRights originalAccessRights)
        {
            var allEnumValues = EnumExtensions.GetValues<FactionMemberAccessRights>();

            var t = originalAccessRights;
            foreach (var value in allEnumValues)
            {
                if (value != FactionMemberAccessRights.Leader)
                {
                    t &= ~value;
                }
            }

            if (t != FactionMemberAccessRights.None)
            {
                // there are some unknown flags defined in the provided access rights
                throw new Exception("Incorrect role access rights: " + originalAccessRights);
            }
        }

        private static bool SharedIsValidRole(FactionMemberRole role)
        {
            return Enum.IsDefined(typeof(FactionMemberRole), role);
        }

        [AssertionMethod]
        private static void SharedValidateDescriptionLength(string text, bool isPrivateDescription)
        {
            if (text.Length
                > (isPrivateDescription
                       ? MaxDescriptionLengthPrivate
                       : MaxDescriptionLengthPublic))
            {
                throw new Exception("Description text length exceeded");
            }
        }

        [AssertionMethod]
        private static void SharedValidateIsOfficerRole(FactionMemberRole role)
        {
            if (role < FactionMemberRole.Officer1
                || role > FactionMemberRole.Officer3)
            {
                throw new Exception("Not an officer role: ");
            }
        }

        private void ServerRemote_LeaderDissolveFaction()
        {
            var currentLeader = ServerRemoteContext.Character;
            var currentLeaderName = currentLeader.Name;

            ServerValidateHasAccessRights(currentLeader,
                                          FactionMemberAccessRights.Leader,
                                          out var faction);

            if (LandClaimSystem.SharedEnumerateAllFactionAreas(SharedGetClanTag(faction))
                               .Any())
            {
                throw new Exception("The faction cannot be dissolved as it still have land claims.");
            }

            Logger.Important("Faction leader dissolved the faction: " + faction, currentLeader);

            // remove all faction members have been removed
            var memberEntries = ServerGetFactionMembersReadOnly(faction);
            while (memberEntries.Count > 0)
            {
                var memberName = memberEntries[0].Name;
                ServerRemoveMemberNoChecks(memberName, faction);
                if (memberName == currentLeaderName)
                {
                    continue;
                }

                try
                {
                    var member = Server.Characters.GetPlayerCharacter(memberName);
                    if (member.ServerIsOnline)
                    {
                        this.CallClient(member,
                                        _ => _.ClientRemote_RemovedFromFaction(currentLeaderName));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                }
            }

            ServerRemoveFaction(faction);
        }

        private void ServerRemote_LeaderSetOfficerRoleTitle(
            FactionMemberRole role,
            FactionOfficerRoleTitle roleTitle)
        {
            var leaderCharacter = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(leaderCharacter,
                                          FactionMemberAccessRights.Leader,
                                          out var faction);
            SharedValidateIsOfficerRole(role);

            var binding = Faction.GetPrivateState(faction).OfficerRoleTitleBinding;
            if (binding.TryGetValue(role, out var currentRoleTitle)
                && currentRoleTitle == roleTitle)
            {
                return;
            }

            binding[role] = roleTitle;
            ServerAddLogEntry(faction,
                              new FactionLogEntryRoleRenamed(byOfficer: leaderCharacter,
                                                             previousRoleTitle: currentRoleTitle,
                                                             newRoleTitle: roleTitle));

            Logger.Important($"Officer role title changed: {role} -> {roleTitle}");
        }

        private void ServerRemote_LeaderSetRoleAccessRights(
            FactionMemberRole role,
            FactionMemberAccessRights accessRights)
        {
            if (role == FactionMemberRole.Member
                || role == FactionMemberRole.Leader
                || !SharedIsValidRole(role))
            {
                throw new Exception("Invalid role: " + role);
            }

            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.Leader,
                                          out var faction);

            ServerValidateRoleAccessRights(accessRights);

            var currentAccessRights = SharedGetRoleAccessRights(faction, role);
            if (currentAccessRights == accessRights)
            {
                // no change required
                return;
            }

            var accessRightsBinding = Faction.GetPrivateState(faction).AccessRightsBinding;
            accessRightsBinding[role] = accessRights;
            Logger.Important($"Changed faction role access rights - {role}: {accessRights}");

            var roleTitle = ServerGetOfficerRoleTitle(faction, role)
                            ?? FactionOfficerRoleTitle.Deputy; // the null should be impossible but still
            ServerAddLogEntry(faction,
                              new FactionLogEntryRoleAccessRightsChanged(byOfficer: officer,
                                                                         roleTitle,
                                                                         accessRights));

            // process all faction members of this role
            foreach (var entry in ServerGetFactionMembersReadOnly(faction))
            {
                if (entry.Role == role)
                {
                    ServerOnFactionMemberAccessRightsChanged(faction,
                                                             entry.Name,
                                                             SharedGetRoleAccessRights(faction, role));
                }
            }
        }

        private void ServerRemote_LeaderTransferOwnership(string newLeaderName)
        {
            var currentLeader = ServerRemoteContext.Character;
            ServerTransferFactionOwnership(currentLeader, newLeaderName);
        }

        [RemoteCallSettings(timeInterval: 0.5)]
        private void ServerRemote_OfficerRemoveFactionMember(string memberName)
        {
            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.RemoveMembers,
                                          out var faction);

            if (string.Equals(memberName, officer.Name, StringComparison.Ordinal))
            {
                throw new Exception("To remove themself the player must use the Leave method");
            }

            var memberEntry = SharedGetMemberEntry(memberName, faction);
            if (memberEntry is null)
            {
                throw new Exception("Faction member is not found: " + memberName);
            }

            if (memberEntry.Value.Role == FactionMemberRole.Leader)
            {
                throw new Exception("Cannot remove the faction leader");
            }

            ServerRemoveMemberNoChecks(memberName, faction);

            var member = Server.Characters.GetPlayerCharacter(memberName);
            this.CallClient(member,
                            _ => _.ClientRemote_RemovedFromFaction(officer.Name));

            ServerAddLogEntry(faction,
                              new FactionLogEntryMemberRemoved(member,
                                                               byOfficer: officer));
        }

        [RemoteCallSettings(timeInterval: 1)]
        private void ServerRemote_OfficerSetDescription(string text, bool isPrivateDescription)
        {
            text = text?.Trim() ?? string.Empty;
            text = SharedTextHelper.TrimSpacesOnEachLine(text);

            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.EditDescription,
                                          out var faction);

            text = ProfanityFilteringSystem.SharedApplyFilters(text);
            SharedValidateDescriptionLength(text, isPrivateDescription);

            var privateState = Faction.GetPrivateState(faction);
            if (isPrivateDescription)
            {
                if (privateState.DescriptionPrivate == text)
                {
                    return;
                }

                privateState.DescriptionPrivate = text;
            }
            else
            {
                if (privateState.DescriptionPublic == text)
                {
                    return;
                }

                privateState.DescriptionPublic = text;
            }

            ServerAddLogEntry(faction, new FactionLogEntryDescriptionChanged(officer));
        }

        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        [RemoteCallSettings(timeInterval: 1)]
        private bool ServerRemote_OfficerSetEmblem(FactionEmblem emblem)
        {
            throw new Exception("This feature is disabled");
#pragma warning disable 162
            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.EditDescription,
                                          out var faction);

            if (!SharedFactionEmblemProvider.SharedIsValidEmblem(emblem))
            {
                throw new Exception("The faction emblem is invalid");
            }

            var factionPublicState = Faction.GetPublicState(faction);
            if (factionPublicState.Emblem.Equals(emblem))
            {
                // this faction already has the same emblem
                return true;
            }

            if (ServerIsEmblemUsed(emblem))
            {
                return false;
            }

            factionPublicState.Emblem = emblem;
            Logger.Important("Faction emblem changed: " + faction, officer);
            return true;
#pragma warning restore 162
        }

        private void ServerRemote_OfficerSetMemberRole(string memberName, FactionMemberRole role)
        {
            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.SetMemberRole,
                                          out var faction);

            if (memberName == officer.Name)
            {
                throw new Exception($"Cannot change self role: {memberName} in {faction}");
            }

            var memberEntry = SharedGetMemberEntry(memberName, faction);
            if (memberEntry is null)
            {
                throw new Exception($"Faction member is not found: {memberName} in {faction}");
            }

            if (memberEntry.Value.Role == FactionMemberRole.Leader)
            {
                throw new Exception("Cannot change the role of the faction leader");
            }

            var member = Server.Characters.GetPlayerCharacter(memberName)
                         ?? throw new Exception("Character not found");

            ServerSetMemberRoleNoChecks(memberName, faction, role);

            var newRoleTitle = ServerGetOfficerRoleTitle(faction, role);
            ServerAddLogEntry(faction,
                              new FactionLogEntryMemberRoleChanged(member,
                                                                   officer,
                                                                   newRole: role,
                                                                   newRoleTitle: newRoleTitle));
        }
    }
}