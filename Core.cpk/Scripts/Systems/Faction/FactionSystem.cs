namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public partial class FactionSystem : ProtoSystem<FactionSystem>
    {
        public const byte FactionListPageSizeMax = 100;

        // TODO: implement proper paging
        public const byte FactionListPageSizeMin = 100;

        public const ushort MaxFactionEventsLogEntries = 500;

        public const ushort MaxFactionEventsLogEntriesRecent = 25;

        private static readonly Lazy<Faction> LazyProtoFaction
            = new(Api.GetProtoEntity<Faction>);

        private static readonly Dictionary<ICharacter, ILogicObject> ServerCharacterFactionDictionary
            = IsServer
                  ? new Dictionary<ICharacter, ILogicObject>()
                  : null;

        private static NetworkSyncList<FactionMemberEntry> clientCurrentFactionMembersList;

        public static readonly ConstructionTileRequirements.Validator ValidatorPlayerHasFaction
            = new(CoreStrings.Faction_ErrorDontHaveFaction,
                  context =>
                  {
                      var forCharacter = context.CharacterBuilder;
                      if (forCharacter is null)
                      {
                          return true;
                      }

                      return IsClient
                                 ? ClientCurrentFaction is not null
                                 : ServerGetFaction(forCharacter) is not null;
                  });

        public delegate void DelegateFactionMemberAccessRightsChanged(
            ILogicObject faction,
            string characterName,
            FactionMemberAccessRights accessRights);

        public delegate void ServerCharacterJoinedOrLeftFactionDelegate(
            ICharacter character,
            ILogicObject faction,
            bool isJoined);

        public static event Action ClientCurrentFactionAccessRightsChanged;

        public static event Action ClientCurrentFactionChanged;

        public static event Action<(FactionMemberEntry entry, bool isAdded)> ClientCurrentFactionMemberAddedOrRemoved;

        public static event Action<string> ClientFactionDissolved;

        public static event ServerCharacterJoinedOrLeftFactionDelegate ServerCharacterJoinedOrLeftFaction;

        public static event Action<string> ServerFactionDissolved;

        public static event DelegateFactionMemberAccessRightsChanged ServerFactionMemberAccessRightsChanged;

        [RemoteEnum]
        public enum CreateFactionResult : byte
        {
            Success = 0,

            InvalidOrTakenClanTag = 10,

            EmblemUsed = 20,

            // already in a faction, or not enough learning points, etc
            Other = byte.MaxValue
        }

        [RemoteEnum]
        public enum FactionListFilter : byte
        {
            AllFactions = 0,

            OnlyWithReceivedInvitations = 1,

            OnlyWithSubmittedApplications = 2,

            Leaderboard = 3
        }

        public static ILogicObject ClientCurrentFaction { get; private set; }

        public static string ClientCurrentFactionClanTag
            => ClientCurrentFaction is null
                   ? null
                   : Faction.GetPublicState(ClientCurrentFaction).ClanTag;

        public static FactionKind ClientCurrentFactionKind
            => ClientCurrentFaction is not null
                   ? Faction.GetPublicState(ClientCurrentFaction).Kind
                   : FactionKind.Private; // fallback value

        public static string ClientCurrentFactionLeaderName
            => Faction.GetPublicState(ClientCurrentFaction).LeaderName;

        public static FactionMemberRole ClientCurrentRole
        {
            get
            {
                var entry = SharedGetMemberEntry(ClientCurrentCharacterHelper.Character.Name, ClientCurrentFaction);
                return entry?.Role ?? FactionMemberRole.Member;
            }
        }

        public static string ClientDefaultJoinCooldownDurationText
            => ClientTimeFormatHelper.FormatTimeDuration(
                FactionConstants.SharedFactionJoinCooldownDuration);

        public static bool ClientHasFaction => ClientCurrentFaction is not null;

        [NotLocalizable]
        public override string Name => "Faction system";

        private static Faction ProtoFaction => LazyProtoFaction.Value;

        public static bool ClientCheckIsUnderJoinCooldown(bool showErrorNotification)
        {
            if (!SharedIsUnderJoinCooldown(ClientCurrentCharacterHelper.Character))
            {
                return false;
            }

            if (showErrorNotification)
            {
                NotificationSystem.ClientShowNotification(
                    title: null,
                    CoreStrings.Faction_ErrorUnderJoinCooldown
                    + "[br]"
                    + CoreStrings.Faction_JoinCooldown_Description,
                    NotificationColor.Bad);
            }

            return true;
        }

        public static Task<CreateFactionResult> ClientCreateFaction(
            FactionKind factionKind,
            string clanTag,
            FactionEmblem emblem)
        {
            if (NewbieProtectionSystem.ClientIsNewbie)
            {
                NewbieProtectionSystem.ClientNotifyNewbieCannotPerformAction(null);
                return Task.FromResult(CreateFactionResult.Other);
            }

            if (ClientCheckIsUnderJoinCooldown(showErrorNotification: true))
            {
                return Task.FromResult(CreateFactionResult.Other);
            }

            Api.Assert(ClientCurrentFaction is null, "Already have a faction");
            return Instance.CallServer(_ => _.ServerRemote_CreateFaction(factionKind,
                                                                         clanTag,
                                                                         emblem));
        }

        [NotNull]
        public static IEnumerable<string> ClientGetCurrentFactionMemberNames()
        {
            if (clientCurrentFactionMembersList is null)
            {
                yield break;
            }

            foreach (var entry in clientCurrentFactionMembersList)
            {
                yield return entry.Name;
            }
        }

        [NotNull]
        public static IReadOnlyList<FactionMemberEntry> ClientGetCurrentFactionMembers()
        {
            return clientCurrentFactionMembersList
                   ?? (IReadOnlyList<FactionMemberEntry>)Array.Empty<FactionMemberEntry>();
        }

        public static FactionOfficerRoleTitle? ClientGetCurrentOfficerRoleTitle()
        {
            var role = ClientCurrentRole;
            if (role == FactionMemberRole.Leader
                || role == FactionMemberRole.Member)
            {
                return null;
            }

            return Faction.GetPrivateState(ClientCurrentFaction)
                          .OfficerRoleTitleBinding.TryGetValue(role, out var entry)
                       ? entry
                       : (FactionOfficerRoleTitle?)null;
        }

        public static Task<FactionEmblem> ClientGetFactionEmblemAsync(string clanTag)
        {
            return Instance.CallServer(_ => _.ServerRemote_GetFactionEmblem(clanTag));
        }

        public static Task<FactionListEntry> ClientGetFactionEntry(string clanTag)
        {
            return Instance.CallServer(
                _ => _.ServerRemote_GetFactionEntry(clanTag));
        }

        public static Task<FactionListPage> ClientGetFactionsList(
            ushort page,
            byte pageSize,
            FactionListSortOrder order,
            string clanTagFilter,
            FactionKind? kindFilter,
            FactionListFilter factionListFilter)
        {
            return Instance.CallServer(
                _ => _.ServerRemote_GetFactionsList(
                    page,
                    pageSize,
                    order,
                    clanTagFilter,
                    kindFilter,
                    factionListFilter));
        }

        public static Task<double> ClientGetLastOnlineDate(string memberName)
        {
            return Instance.CallServer(_ => _.ServerRemote_GetLastOnlineDate(memberName));
        }

        public static string ClientGetRoleTitle(FactionMemberRole role)
        {
            return role switch
            {
                FactionMemberRole.Leader => CoreStrings.Faction_Role_Leader,
                FactionMemberRole.Member => CoreStrings.Faction_Role_Member,
                _ => Faction.GetPrivateState(ClientCurrentFaction)
                            .OfficerRoleTitleBinding.TryGetValue(role, out var entry)
                         ? entry.GetDescription()
                         : role.ToString()
            };
        }

        public static bool ClientHasAccessRight(FactionMemberAccessRights accessRights)
        {
            return SharedHasAccessRight(ClientCurrentCharacterHelper.Character.Name,
                                        ClientCurrentFaction,
                                        accessRights);
        }

        public static bool ClientIsFactionMember(string name)
        {
            if (clientCurrentFactionMembersList is null)
            {
                return false;
            }

            foreach (var entry in clientCurrentFactionMembersList)
            {
                if (!string.Equals(entry.Name, name, StringComparison.Ordinal))
                {
                    continue;
                }

                // found the member
                return true;
            }

            // no such member
            return false;
        }

        public static bool ClientIsFactionMemberOrAlly(ICharacter character)
        {
            if (ClientCurrentFaction is null
                || character is null
                || character.IsNpc
                || !character.IsInitialized)
            {
                return false;
            }

            var clanTag = SharedGetClanTag(character);
            if (ClientCurrentFactionClanTag == clanTag)
            {
                // faction member
                return true;
            }

            return SharedGetFactionDiplomacyStatus(ClientCurrentFaction, clanTag)
                   == FactionDiplomacyStatus.Ally;
        }

        public static void ClientJoin(string clanTag)
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

            DialogWindow.ShowDialog(
                title: CoreStrings.Faction_Join,
                string.Format(CoreStrings.Faction_DialogJoinConfirmation_Message_Format,
                              @$"\[{clanTag}\]")
                + "[br]"
                + "[br]"
                + CoreStrings.Faction_JoinCooldown_Description
                + " ("
                + ClientDefaultJoinCooldownDurationText
                + ")",
                okAction: async () =>
                          {
                              var result = await Instance.CallServer(_ => _.ServerRemote_JoinFaction(clanTag));
                              if (result != ApplicationCreateResult.Success)
                              {
                                  NotificationSystem.ClientShowNotification(
                                                        title: CoreStrings.Faction_Title,
                                                        result.GetDescription(),
                                                        NotificationColor.Bad)
                                                    .ViewModel
                                                    .Icon = ClientFactionEmblemCache.GetEmblemTextureBrush(clanTag);
                              }
                          },
                okText: CoreStrings.Faction_Join,
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        public static void ClientLeaveFaction()
        {
            Api.Assert(ClientCurrentFaction is not null, "Don't have a faction");
            Instance.CallServer(_ => _.ServerRemote_LeaveFaction());
        }

        public static async void ClientOpenPrivateChatWithFactionLeader(string clanTag)
        {
            var leaderName = await Instance.CallServer(
                                 _ => _.ServerRemote_GetFactionLeaderName(clanTag));

            Menu.CloseAll();
            WindowsManager.LastOpenedWindow?.Close(DialogResult.Cancel);

            ChatSystem.ClientOpenPrivateChat(withCharacterName: leaderName);
        }

        public static void ClientUpgradeFactionLevel()
        {
            var toLevel = (byte)(Faction.GetPublicState(ClientCurrentFaction).Level + 1);
            if (toLevel > FactionConstants.MaxFactionLevel)
            {
                throw new Exception("Cannot upgrade beyond max level");
            }

            var upgradeCost = FactionConstants.SharedGetFactionUpgradeCost(toLevel);
            if (ClientCurrentCharacterHelper.PrivateState.Technologies.LearningPoints
                < upgradeCost)
            {
                throw new Exception("Not enough learning points");
            }

            DialogWindow.ShowDialog(
                title: CoreStrings.QuestionAreYouSure,
                text: string.Format(CoreStrings.LearningPointsCost_Format, upgradeCost),
                okAction: () => Instance.CallServer(
                              _ => _.ServerRemote_UpgradeFactionLevel(toLevel)),
                okText: CoreStrings.Button_Upgrade,
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        public static CreateFactionResult ServerCreateFaction(
            ICharacter character,
            FactionKind factionKind,
            string clanTag,
            FactionEmblem emblem)
        {
            Api.Assert(!character.IsNpc, "NPC cannot create a faction");

            var faction = ServerGetFaction(character);
            if (faction is not null)
            {
                throw new Exception($"Player already has a faction: {character} in {faction}");
            }

            if (NewbieProtectionSystem.SharedIsNewbie(character)
                || SharedIsUnderJoinCooldown(character))
            {
                // should be impossible as client performs the same check
                return CreateFactionResult.Other;
            }

            if (!SharedFactionEmblemProvider.SharedIsValidEmblem(emblem))
            {
                throw new Exception("The faction emblem is invalid");
            }

            if (FactionConstants.GetFactionMembersMax(factionKind) == 0)
            {
                throw new Exception("This faction kind is disabled by server settings");
            }

            clanTag = clanTag.ToUpperInvariant();
            if (!SharedIsValidClanTag(clanTag))
            {
                return CreateFactionResult.InvalidOrTakenClanTag;
            }

            var otherFaction = ServerFindFactionByClanTag(clanTag);
            if (otherFaction is not null)
            {
                return CreateFactionResult.InvalidOrTakenClanTag;
            }

            var technologies = character.SharedGetTechnologies();
            if (technologies.LearningPoints < FactionConstants.SharedCreateFactionLearningPointsCost)
            {
                // should be impossible as client performs the same check
                return CreateFactionResult.Other;
            }

            if (ServerIsEmblemUsed(emblem))
            {
                return CreateFactionResult.EmblemUsed;
            }

            technologies.ServerRemoveLearningPoints(FactionConstants.SharedCreateFactionLearningPointsCost);

            faction = Server.World.CreateLogicObject<Faction>();
            var publicState = Faction.GetPublicState(faction);
            publicState.Kind = factionKind;
            publicState.Emblem = emblem;
            publicState.LeaderName = character.Name;
            publicState.ClanTag = clanTag;

            Logger.Important($"Created faction: {faction} with leader {character}. Kind: {factionKind}", character);

            ServerAddLogEntry(faction, new FactionLogEntryFactionCreated(character));
            ServerAddMember(character, faction, FactionMemberRole.Leader);
            return CreateFactionResult.Success;
        }

        public static ILogicObject ServerFindFactionByClanTag(string clanTag)
        {
            if (string.IsNullOrEmpty(clanTag))
            {
                return null;
            }

            var allParties = Server.World.GetGameObjectsOfProto<ILogicObject, Faction>();
            foreach (var faction in allParties)
            {
                if (string.Equals(clanTag,
                                  Faction.GetPublicState(faction).ClanTag,
                                  StringComparison.OrdinalIgnoreCase))
                {
                    return faction;
                }
            }

            return null;
        }

        [CanBeNull]
        public static ILogicObject ServerGetFaction(ICharacter character)
        {
            return ServerCharacterFactionDictionary.Find(character);
        }

        public static ILogicObject ServerGetFactionByClanTag(
            string clanTag,
            bool throwExceptionIfNull = true)
        {
            if (string.IsNullOrEmpty(clanTag))
            {
                return throwExceptionIfNull
                           ? throw new Exception("Faction is not found: clanTag=<null>")
                           : (ILogicObject)null;
            }

            foreach (var faction in ServerGetFactionsTempList().EnumerateAndDispose())
            {
                if (!string.Equals(clanTag,
                                   SharedGetClanTag(faction),
                                   StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return faction;
            }

            return throwExceptionIfNull
                       ? throw new Exception("Faction is not found: clanTag=" + clanTag)
                       : (ILogicObject)null;
        }

        public static ILogicObject ServerGetFactionChat(ILogicObject faction)
        {
            return Faction.GetPrivateState(faction)
                          .ServerFactionChatHolder;
        }

        public static IEnumerable<string> ServerGetFactionMemberNames(ILogicObject faction)
        {
            return faction is not null
                       ? ServerGetFactionMembersEditable(faction).Select(m => m.Name)
                       : Array.Empty<string>();
        }

        public static IEnumerable<string> ServerGetFactionMemberNames(ICharacter character)
        {
            var faction = ServerGetFaction(character);
            return ServerGetFactionMemberNames(faction);
        }

        public static IReadOnlyList<FactionMemberEntry> ServerGetFactionMembersReadOnly(ICharacter character)
        {
            var faction = ServerGetFaction(character);
            return ServerGetFactionMembersReadOnly(faction);
        }

        public static IReadOnlyList<FactionMemberEntry> ServerGetFactionMembersReadOnly(ILogicObject faction)
        {
            return faction is not null
                       ? ServerGetFactionMembersEditable(faction)
                       : (IReadOnlyList<FactionMemberEntry>)Array.Empty<FactionMemberEntry>();
        }

        /// <summary>
        /// It will return null if the role is not an officer.
        /// </summary>
        public static FactionOfficerRoleTitle? ServerGetOfficerRoleTitle(ILogicObject faction, FactionMemberRole role)
        {
            return role switch
            {
                FactionMemberRole.Leader => null,
                FactionMemberRole.Member => null,
                _ => Faction.GetPrivateState(faction)
                            .OfficerRoleTitleBinding.TryGetValue(role, out var entry)
                         ? entry
                         : (FactionOfficerRoleTitle?)null
            };
        }

        public static FactionMemberRole ServerGetRole(ICharacter character)
        {
            var faction = ServerGetFaction(character);
            if (faction is null)
            {
                throw new Exception("Player has no faction: " + character);
            }

            var characterName = character.Name;
            var memberEntries = ServerGetFactionMembersReadOnly(faction);
            foreach (var entry in memberEntries)
            {
                if (!string.Equals(characterName, entry.Name, StringComparison.Ordinal))
                {
                    continue;
                }

                return entry.Role;
            }

            // member not found (should be impossible)
            throw new InvalidOperationException();
        }

        public static bool ServerHasAccessRights(
            ICharacter character,
            FactionMemberAccessRights accessRights,
            out ILogicObject faction)
        {
            faction = ServerGetFaction(character);
            if (faction is null)
            {
                return false;
            }

            return SharedHasAccessRight(character.Name,
                                        faction,
                                        accessRights);
        }

        public static void ServerLeaveFaction(ICharacter character)
        {
            var faction = ServerGetFaction(character);
            if (faction is null)
            {
                throw new Exception("Not a member of any faction");
            }

            ServerRemoveMemberNoChecks(character.Name, faction);
            ServerAddLogEntry(faction,
                              new FactionLogEntryMemberLeft(character));
        }

        public static void ServerOnLandClaimDecayed(ILogicObject faction, Vector2Ushort centerTilePosition)
        {
            ServerAddLogEntry(faction,
                              new FactionLogEntryLandClaimDecayed(centerTilePosition));
        }

        public static void ServerOnLandClaimExpanded(
            ILogicObject faction,
            Vector2Ushort worldPosition,
            [CanBeNull] ICharacter byMember)
        {
            ServerAddLogEntry(faction,
                              new FactionLogEntryTerritoryAltered(byMember, worldPosition, isExpanded: true));
        }

        public static void ServerOnLandClaimRemoved(
            ILogicObject faction,
            Vector2Ushort worldPosition,
            [CanBeNull] ICharacter byMember)
        {
            ServerAddLogEntry(faction,
                              new FactionLogEntryTerritoryAltered(byMember, worldPosition, isExpanded: false));
        }

        public static void ServerValidateHasAccessRights(
            ICharacter character,
            FactionMemberAccessRights accessRights,
            out ILogicObject faction)
        {
            if (ServerHasAccessRights(character,
                                      accessRights,
                                      out faction))
            {
                return;
            }

            if (faction is null)
            {
                throw new Exception("Player don't have a faction");
            }

            throw new Exception("Player doesn't have a faction access right: " + accessRights);
        }

        public static void ServerValidateHasAccessRights(
            ICharacter character,
            FactionMemberAccessRights accessRights)
        {
            ServerValidateHasAccessRights(character, accessRights, out _);
        }

        public static bool SharedArePlayersInTheSameFaction(ICharacter characterA, ICharacter characterB)
        {
            if (characterA.IsNpc
                || characterB.IsNpc)
            {
                return false;
            }

            var clanTagCharacterA = SharedGetClanTag(characterA);
            return !string.IsNullOrEmpty(clanTagCharacterA)
                   && clanTagCharacterA == SharedGetClanTag(characterB);
        }

        public static string SharedGetClanTag(ILogicObject faction)
        {
            return faction is null
                       ? null
                       : Faction.GetPublicState(faction).ClanTag;
        }

        public static string SharedGetClanTag(ICharacter character)
        {
            return character is null || character.IsNpc
                       ? null
                       : PlayerCharacter.GetPublicState(character).ClanTag;
        }

        public static FactionKind SharedGetFactionKind(ILogicObject faction)
        {
            return Faction.GetPublicState(faction).Kind;
        }

        public static FactionMemberEntry? SharedGetMemberEntry(string characterName, ILogicObject faction)
        {
            if (faction is null)
            {
                return null;
            }

            IReadOnlyList<FactionMemberEntry> memberEntries;

            if (IsServer)
            {
                if (faction
                    != ServerGetFaction(
                        Server.Characters.GetPlayerCharacter(characterName)))
                {
                    // not a faction member
                    return null;
                }

                memberEntries = ServerGetFactionMembersReadOnly(faction);
            }
            else
            {
                if (ClientCurrentFaction is null
                    || ClientCurrentFaction.IsDestroyed
                    || ClientCurrentFaction != faction)
                {
                    return null;
                }

                memberEntries = clientCurrentFactionMembersList;
            }

            foreach (var entry in memberEntries)
            {
                if (!string.Equals(characterName, entry.Name, StringComparison.Ordinal))
                {
                    continue;
                }

                return entry;
            }

            // member not found (possible only on the client side)
            return null;
        }

        public static FactionMemberAccessRights SharedGetRoleAccessRights(ILogicObject faction, FactionMemberRole role)
        {
            return faction.GetPrivateState<FactionPrivateState>()
                          .AccessRightsBinding.TryGetValue(role, out var roleAccessRights)
                       ? roleAccessRights
                       : FactionMemberAccessRights.None;
        }

        public static bool SharedHasAccessRight(
            ICharacter character,
            FactionMemberAccessRights accessRights)
        {
            if (accessRights == FactionMemberAccessRights.None)
            {
                return true;
            }

            var faction = IsServer
                              ? ServerGetFaction(character)
                              : ClientCurrentFaction; // client knows only about the current faction
            var entry = SharedGetMemberEntry(character.Name, faction);
            return entry.HasValue
                   && SharedHasAccessRights(faction, entry.Value.Role, accessRights);
        }

        public static bool SharedHasAccessRight(
            string characterName,
            ILogicObject faction,
            FactionMemberAccessRights accessRights)
        {
            if (accessRights == FactionMemberAccessRights.None)
            {
                return true;
            }

            var entry = SharedGetMemberEntry(characterName, faction);
            return entry.HasValue
                   && SharedHasAccessRights(faction, entry.Value.Role, accessRights);
        }

        public static bool SharedIsUnderJoinCooldown(ICharacter character)
        {
            var lastFactionLeaveTime = PlayerCharacter.GetPrivateState(character).LastFactionLeaveTime;
            if (lastFactionLeaveTime <= 0)
            {
                return false;
            }

            var time = IsServer
                           ? Server.Game.FrameTime
                           : Client.CurrentGame.ServerFrameTimeApproximated;

            return lastFactionLeaveTime + FactionConstants.SharedFactionJoinCooldownDuration
                   > time;
        }

        [RemoteCallSettings(timeInterval: 0.05, clientMaxSendQueueSize: 250)]
        public FactionEmblem ServerRemote_GetFactionEmblem(string clanTag)
        {
            var faction = ServerGetFactionByClanTag(clanTag);
            return Faction.GetPublicState(faction).Emblem;
        }

        [RemoteCallSettings(timeInterval: 1)]
        public FactionListEntry ServerRemote_GetFactionEntry(string clanTag)
        {
            var faction = ServerGetFactionByClanTag(clanTag);
            return ServerCreateFactionListEntry(faction);
        }

        internal static void ServerRegisterFaction(ILogicObject faction)
        {
            var membersEntries = ServerGetFactionMembersReadOnly(faction);
            foreach (var entry in membersEntries)
            {
                var character = Server.Characters.GetPlayerCharacter(entry.Name);
                if (character is null)
                {
                    Logger.Warning("Unknown character in faction: " + entry);
                    continue;
                }

                if (ServerCharacterFactionDictionary.TryGetValue(character, out var existingFaction))
                {
                    Logger.Warning($"Player is already in faction: {entry} from {existingFaction} - will remove",
                                   character);
                    ServerRemoveMemberNoChecks(entry.Name, existingFaction);
                }

                ServerCharacterFactionDictionary[character] = faction;
            }
        }

        protected override void PrepareSystem()
        {
            FactionConstants.EnsureInitialized();

            if (IsClient)
            {
                return;
            }

            Server.Characters.PlayerNameChanged += ServerPlayerNameChangedHandler;
            Server.Characters.PlayerOnlineStateChanged += ServerPlayerOnlineStateChangedHandler;
            LandClaimSystem.ServerRaidBlockStartedOrExtended +=
                this.ServerLandClaimAreaRaidBlockStartedOrExtendedHandler;
        }

        private static void ClientCurrentFactionMemberInsertedHandler(
            NetworkSyncList<FactionMemberEntry> list,
            int index,
            FactionMemberEntry entry)
        {
            Api.SafeInvoke(
                () => ClientCurrentFactionMemberAddedOrRemoved?.Invoke((entry, isAdded: true)));
        }

        private static void ClientCurrentFactionMemberRemovedHandler(
            NetworkSyncList<FactionMemberEntry> list,
            int index,
            FactionMemberEntry entry)
        {
            Api.SafeInvoke(
                () => ClientCurrentFactionMemberAddedOrRemoved?.Invoke((entry, isAdded: false)));
        }

        private static void ClientSetCurrentFaction(ILogicObject faction)
        {
            Logger.Important("Current faction received: " + (faction?.ToString() ?? "<no faction>"));
            if (ClientCurrentFaction == faction)
            {
                Api.SafeInvoke(() => ClientCurrentFactionAccessRightsChanged?.Invoke());
                return;
            }

            var handlerFactionMemberAddedOrRemoved = ClientCurrentFactionMemberAddedOrRemoved;
            var handlerDiplomacyStatusChanged = ClientFactionDiplomacyStatusChanged;

            ClientCurrentFaction = null;
            PurgeMembersList();
            PurgeDiplomacyDictionary();
            ClientFactionEventsLogListener.Reset();

            ClientCurrentFaction = faction;
            SetupMembersList();
            SetupDiplomacyDictionary();
            ClientFactionEventsLogListener.Setup(faction);

            Api.SafeInvoke(() => ClientCurrentFactionChanged?.Invoke());
            Api.SafeInvoke(() => ClientCurrentFactionAccessRightsChanged?.Invoke());

            ProcessMembersList();
            ProcessDiplomacyDictionary();

            void PurgeMembersList()
            {
                var previousFactionMembersList = clientCurrentFactionMembersList;
                if (clientCurrentFactionMembersList is not null)
                {
                    clientCurrentFactionMembersList.ClientElementInserted -= ClientCurrentFactionMemberInsertedHandler;
                    clientCurrentFactionMembersList.ClientElementRemoved -= ClientCurrentFactionMemberRemovedHandler;
                    clientCurrentFactionMembersList = null;
                }

                if (handlerFactionMemberAddedOrRemoved is not null
                    && previousFactionMembersList is not null)
                {
                    foreach (var member in previousFactionMembersList)
                    {
                        Api.SafeInvoke(() => handlerFactionMemberAddedOrRemoved((member, isAdded: false)));
                    }
                }
            }

            void PurgeDiplomacyDictionary()
            {
                var previousFactionDiplomacyStatuses = clientCurrentFactionDiplomacyStatuses;
                if (clientCurrentFactionDiplomacyStatuses is not null)
                {
                    clientCurrentFactionDiplomacyStatuses.ClientPairRemoved
                        -= ClientCurrentFactionDiplomacyStatusesPairRemovedHandler;
                    clientCurrentFactionDiplomacyStatuses.ClientPairSet
                        -= CurrentFactionDiplomacyStatusesPairSetHandler;
                    clientCurrentFactionDiplomacyStatuses = null;
                }

                if (handlerDiplomacyStatusChanged is not null
                    && previousFactionDiplomacyStatuses is not null)
                {
                    foreach (var clanTag in previousFactionDiplomacyStatuses.Keys)
                    {
                        Api.SafeInvoke(() => handlerDiplomacyStatusChanged((clanTag, FactionDiplomacyStatus.Neutral)));
                    }
                }
            }

            void SetupMembersList()
            {
                clientCurrentFactionMembersList = faction is not null
                                                      ? Faction.GetPrivateState(faction).Members
                                                      : null;

                if (clientCurrentFactionMembersList is not null)
                {
                    clientCurrentFactionMembersList.ClientElementInserted += ClientCurrentFactionMemberInsertedHandler;
                    clientCurrentFactionMembersList.ClientElementRemoved += ClientCurrentFactionMemberRemovedHandler;
                }
            }

            void SetupDiplomacyDictionary()
            {
                clientCurrentFactionDiplomacyStatuses = faction is not null
                                                            ? Faction.GetPrivateState(faction).FactionDiplomacyStatuses
                                                            : null;

                if (clientCurrentFactionDiplomacyStatuses is not null)
                {
                    clientCurrentFactionDiplomacyStatuses.ClientPairRemoved
                        += ClientCurrentFactionDiplomacyStatusesPairRemovedHandler;
                    clientCurrentFactionDiplomacyStatuses.ClientPairSet
                        += CurrentFactionDiplomacyStatusesPairSetHandler;
                }
            }

            void ProcessMembersList()
            {
                if (handlerFactionMemberAddedOrRemoved is not null
                    && clientCurrentFactionMembersList is not null)
                {
                    foreach (var member in clientCurrentFactionMembersList)
                    {
                        Api.SafeInvoke(() => handlerFactionMemberAddedOrRemoved((member, isAdded: true)));
                    }
                }
            }

            void ProcessDiplomacyDictionary()
            {
                if (handlerDiplomacyStatusChanged is not null
                    && clientCurrentFactionDiplomacyStatuses is not null)
                {
                    foreach (var pair in clientCurrentFactionDiplomacyStatuses)
                    {
                        Api.SafeInvoke(() => handlerDiplomacyStatusChanged((pair.Key, pair.Value)));
                    }
                }
            }
        }

        private static void ClientValidateHasAccessRights(FactionMemberAccessRights accessRights)
        {
            Api.Assert(ClientCurrentFaction is not null, "Don't have a faction");
            Api.Assert(SharedHasAccessRight(ClientCurrentCharacterHelper.Character.Name,
                                            ClientCurrentFaction,
                                            accessRights),
                       "You don't have an access right: " + accessRights);
        }

        private static void SendPrivateChatServiceMessage(
            ICharacter aboutCharacter,
            ILogicObject faction,
            string message)
        {
            var serverCharacters = Server.Characters;
            foreach (var entry in ServerGetFactionMembersReadOnly(faction))
            {
                var memberCharacter = serverCharacters.GetPlayerCharacter(entry.Name);
                if (memberCharacter is null
                    || ReferenceEquals(aboutCharacter, memberCharacter))
                {
                    continue;
                }

                var privateChat = ChatSystem.ServerFindPrivateChat(aboutCharacter, entry.Name);
                if (privateChat is not null)
                {
                    ChatSystem.ServerSendServiceMessage(privateChat,
                                                        message,
                                                        forCharacterName: aboutCharacter.Name,
                                                        customDestinationCharacter: memberCharacter);
                }
            }
        }

        private static void ServerAddLogEntry(ILogicObject faction, BaseFactionEventLogEntry logEntry)
        {
            var privateState = Faction.GetPrivateState(faction);
            privateState.ServerEventsLog.Add(logEntry);
            privateState.RecentEventsLog.Add(logEntry);

            while (privateState.ServerEventsLog.Count > MaxFactionEventsLogEntries)
            {
                privateState.ServerEventsLog.RemoveAt(0);
            }

            while (privateState.RecentEventsLog.Count > MaxFactionEventsLogEntriesRecent)
            {
                privateState.RecentEventsLog.RemoveAt(0);
            }
        }

        private static FactionListEntry ServerCreateFactionListEntry(ILogicObject faction)
        {
            var publicState = Faction.GetPublicState(faction);
            var privateState = Faction.GetPrivateState(faction);
            var factionListEntry = new FactionListEntry(
                publicState.ClanTag,
                publicState.Kind,
                publicState.IsAcceptingApplications,
                publicState.LeaderName,
                privateState.DescriptionPublic,
                publicState.PlayersNumberCurrent,
                publicState.Level,
                publicState.LeaderboardRank,
                publicState.TotalScore);
            return factionListEntry;
        }

        private static IEnumerable<ICharacter> ServerEnumerateFactionOfficers(
            ILogicObject faction,
            FactionMemberAccessRights accessRights)
        {
            return ServerGetFactionMembersReadOnly(faction)
                   .Where(m => SharedHasAccessRights(faction, m.Role, accessRights))
                   .Select(m => Server.Characters.GetPlayerCharacter(m.Name));
        }

        private static NetworkSyncList<FactionMemberEntry> ServerGetFactionMembersEditable(ILogicObject faction)
        {
            return Faction.GetPrivateState(faction)
                          .Members;
        }

        private static ITempList<ILogicObject> ServerGetFactionsTempList()
        {
            var tempListFactions = Api.Shared.GetTempList<ILogicObject>();
            ProtoFaction.GetAllGameObjects(tempListFactions.AsList());
            return tempListFactions;
        }

        private static Comparison<ILogicObject> ServerGetSortMethod(FactionListSortOrder order)
        {
            return order switch
            {
                FactionListSortOrder.LeaderboardRank => CompareByRank,
                FactionListSortOrder.MembersNumber => CompareByPlayersNumberCurrent,
                FactionListSortOrder.Level => CompareByLevel,
                _ => throw new ArgumentOutOfRangeException(nameof(order), order, null)
            };

            static int CompareByRank(ILogicObject a, ILogicObject b)
                // reverse comparison (lower rank number comes first)
                => Faction.GetPublicState(a).LeaderboardRank
                          .CompareTo(Faction.GetPublicState(b).LeaderboardRank);

            static int CompareByLevel(ILogicObject a, ILogicObject b)
                // reverse comparison (higher level comes first)
                => Faction.GetPublicState(b).Level
                          .CompareTo(Faction.GetPublicState(a).Level);

            static int CompareByPlayersNumberCurrent(ILogicObject a, ILogicObject b)
                // reverse comparison (higher players number comes first)
                => Faction.GetPublicState(b).PlayersNumberCurrent
                          .CompareTo(Faction.GetPublicState(a).PlayersNumberCurrent);
        }

        private static bool ServerIsEmblemUsed(FactionEmblem emblem)
        {
            foreach (var faction in ServerGetFactionsTempList().EnumerateAndDispose())
            {
                if (emblem.Equals(Faction.GetPublicState(faction).Emblem))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ServerPlayerNameChangedHandler(string oldName, string newName)
        {
            foreach (var faction in ServerCharacterFactionDictionary.Values)
            {
                var factionPublicState = Faction.GetPublicState(faction);
                if (factionPublicState.LeaderName == oldName)
                {
                    factionPublicState.LeaderName = newName;
                    Logger.Important(
                        $"Replaced faction leader username due to username change: {oldName}->{newName} in {faction}");
                }

                var members = Faction.GetPrivateState(faction).Members;

                for (var index = 0; index < members.Count; index++)
                {
                    var entry = members[index];
                    if (!string.Equals(entry.Name, oldName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    members[index] = new FactionMemberEntry(newName, entry.Role);
                    Logger.Important(
                        $"Replaced faction member username due to username change: {oldName}->{newName} in {faction}");
                    break;
                }
            }
        }

        private static void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            if (!isOnline)
            {
                return;
            }

            Instance.CallClient(character,
                                _ => _.ClientRemote_SetSystemConstants(
                                    FactionConstants.GetFactionMembersMax(FactionKind.Public),
                                    FactionConstants.GetFactionMembersMax(FactionKind.Private),
                                    FactionConstants.SharedCreateFactionLearningPointsCost,
                                    FactionConstants.SharedFactionJoinCooldownDuration,
                                    FactionConstants.SharedFactionLandClaimsPerLevel,
                                    FactionConstants.SharedPvpAlliancesEnabled));

            ServerSendCurrentFaction(character);

            var faction = ServerGetFaction(character);
            if (faction is null)
            {
                ServerInvitations.SyncAllReceivedInvitationsList(invitee: character);
                ServerApplications.SyncAllSubmittedApplicationsList(applicant: character);
            }
            else if (SharedHasAccessRight(character.Name,
                                          faction,
                                          FactionMemberAccessRights.Recruitment))
            {
                ServerInvitations.OfficerSyncInvitationsList(officer: character, faction);
                ServerApplications.OfficerSyncApplicationsList(officer: character, faction);
            }
        }

        private static void ServerRemoveFaction(ILogicObject faction)
        {
            var memberEntries = ServerGetFactionMembersReadOnly(faction);
            // ensure all members have been removed
            while (memberEntries.Count > 0)
            {
                ServerRemoveMemberNoChecks(memberEntries[0].Name, faction);
            }

            if (faction.IsDestroyed)
            {
                return;
            }

            ServerInvitations.RemoveAllInvitationsInFaction(faction);
            ServerApplications.RemoveAllApplicationsInFaction(faction);
            ServerResetDiplomacy(faction);

            var factionPublicState = Faction.GetPublicState(faction);
            var clanTag = factionPublicState.ClanTag;
            factionPublicState.ClanTag = null;
            Server.World.DestroyObject(ServerGetFactionChat(faction));
            Server.World.DestroyObject(faction);
            Logger.Important("Faction destroyed: " + faction);

            Api.SafeInvoke(() => ServerFactionDissolved?.Invoke(clanTag));

            Instance.CallClient(
                Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                _ => _.ClientRemote_OnFactionDissolved(clanTag));
        }

        private static void ServerSendCurrentFaction(ICharacter character)
        {
            var faction = ServerGetFaction(character);
            if (faction is null)
            {
                Instance.CallClient(character, _ => _.ClientRemote_CurrentFaction(null));
                return;
            }

            ChatSystem.ServerAddChatRoomToPlayerScope(character, ServerGetFactionChat(faction));

            Server.World.EnterPrivateScope(character, faction);
            Server.World.ForceEnterScope(character, faction);

            Instance.CallClient(character, _ => _.ClientRemote_CurrentFaction(faction));
        }

        private static bool SharedHasAccessRights(
            ILogicObject faction,
            FactionMemberRole role,
            FactionMemberAccessRights accessRights)
        {
            if (accessRights == FactionMemberAccessRights.None)
            {
                return true;
            }

            var roleAccessRights = SharedGetRoleAccessRights(faction, role);
            return roleAccessRights.HasFlag(accessRights);
        }

        private void ClientRemote_CurrentFaction([CanBeNull] ILogicObject faction)
        {
            ClientSetCurrentFaction(faction);
        }

        private void ClientRemote_OnFactionDissolved(string clanTag)
        {
            Logger.Important("Faction dissolved: " + clanTag);
            Api.SafeInvoke(() => ClientFactionDissolved?.Invoke(clanTag));
        }

        private void ClientRemote_RemovedFromFaction(string officerName)
        {
            NotificationSystem.ClientShowNotification(
                                  title: CoreStrings.Faction_Title,
                                  message: string.Format(
                                      CoreStrings.Faction_NotificationCurrentPlayerRemoved_ByOfficer_Format,
                                      officerName))
                              .HideAfterDelay(60);
        }

        private void ClientRemote_SetSystemConstants(
            ushort publicFactionMembersMax,
            ushort privateFactionMembersMax,
            ushort createFactionLpCost,
            uint factionJoinCooldownDuration,
            float factionLandClaimsPerLevel,
            bool pvpAlliancesEnabled)
        {
            FactionConstants.ClientSetSystemConstants(publicFactionMembersMax,
                                                      privateFactionMembersMax,
                                                      createFactionLpCost,
                                                      factionJoinCooldownDuration,
                                                      factionLandClaimsPerLevel,
                                                      pvpAlliancesEnabled);
        }

        private void ServerLandClaimAreaRaidBlockStartedOrExtendedHandler(
            ILogicObject area,
            ICharacter raiderCharacter,
            bool isNewRaidBlock)
        {
            if (!isNewRaidBlock)
            {
                // do not log the raid block extension event
                return;
            }

            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(area);
            var faction = LandClaimAreasGroup.GetPublicState(areasGroup).ServerFaction;
            if (faction is null)
            {
                return;
            }

            var centerTilePosition = LandClaimSystem.SharedGetLandClaimGroupCenterPosition(areasGroup);
            ServerAddLogEntry(
                faction,
                new FactionLogEntryFactionBaseRaided(centerTilePosition, raiderCharacter));
        }

        [RemoteCallSettings(timeInterval: 2)]
        private CreateFactionResult ServerRemote_CreateFaction(
            FactionKind factionKind,
            string clanTag,
            FactionEmblem emblem)
        {
            return ServerCreateFaction(ServerRemoteContext.Character,
                                       factionKind,
                                       clanTag,
                                       emblem);
        }

        [RemoteCallSettings(timeInterval: 1, deliveryMode: DeliveryMode.ReliableSequenced)]
        private string ServerRemote_GetFactionLeaderName(string clanTag)
        {
            var faction = ServerGetFactionByClanTag(clanTag);
            return Faction.GetPublicState(faction).LeaderName;
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 0.5)]
        private FactionListPage ServerRemote_GetFactionsList(
            ushort page,
            byte pageSize,
            FactionListSortOrder order,
            string clanTagFilter,
            FactionKind? kindFilter,
            FactionListFilter factionListFilter)
        {
            Api.Assert(pageSize >= FactionListPageSizeMin
                       && pageSize <= FactionListPageSizeMax,
                       "Page size exceeded");

            var character = ServerRemoteContext.Character;

            using var tempAllFactions = ServerGetFactionsTempList();
            var list = tempAllFactions.AsList();

            // apply kind filter
            if (kindFilter.HasValue)
            {
                var filter = kindFilter.Value;
                for (var index = 0; index < list.Count; index++)
                {
                    var faction = list[index];
                    var kind = Faction.GetPublicState(faction).Kind;
                    if (filter == kind)
                    {
                        continue;
                    }

                    list.RemoveAt(index);
                    index--;
                }
            }

            // apply clan tag filter
            clanTagFilter = (clanTagFilter ?? string.Empty).Trim().ToUpperInvariant();
            if (clanTagFilter.Length > 0)
            {
                for (var index = 0; index < list.Count; index++)
                {
                    var faction = list[index];
                    var clanTag = Faction.GetPublicState(faction).ClanTag;
                    if (clanTag.IndexOf(clanTagFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }

                    list.RemoveAt(index);
                    index--;
                }
            }

            if (factionListFilter != FactionListFilter.AllFactions)
            {
                switch (factionListFilter)
                {
                    case FactionListFilter.Leaderboard:
                        // find only private factions with score above 0
                        for (var index = 0; index < list.Count; index++)
                        {
                            var faction = list[index];
                            var publicState = Faction.GetPublicState(faction);
                            if (publicState.Kind == FactionKind.Private
                                && publicState.TotalScore > 0)
                            {
                                continue;
                            }

                            list.RemoveAt(index);
                            index--;
                        }

                        break;

                    case FactionListFilter.OnlyWithReceivedInvitations:
                    {
                        // find factions with invitations for current player
                        for (var index = 0; index < list.Count; index++)
                        {
                            var isInvitationFound = false;
                            var faction = list[index];
                            foreach (var invitation in ServerInvitations.GetInvitations(faction))
                            {
                                if (!ReferenceEquals(invitation.Invitee, character))
                                {
                                    continue;
                                }

                                isInvitationFound = true;
                                break;
                            }

                            if (!isInvitationFound)
                            {
                                list.RemoveAt(index);
                                index--;
                            }
                        }

                        break;
                    }
                    case FactionListFilter.OnlyWithSubmittedApplications:
                    {
                        // find factions with applications by current player
                        for (var index = 0; index < list.Count; index++)
                        {
                            var isApplicationFound = false;
                            var faction = list[index];
                            foreach (var application in ServerApplications.GetApplications(faction))
                            {
                                if (!ReferenceEquals(application.Applicant, character))
                                {
                                    continue;
                                }

                                isApplicationFound = true;
                                break;
                            }

                            if (!isApplicationFound)
                            {
                                list.RemoveAt(index);
                                index--;
                            }
                        }

                        break;
                    }
                }
            }

            // calculate the result chunk bounds 
            var totalEntriesCount = list.Count;
            var startIndex = page * pageSize;
            var endIndex = startIndex + pageSize;
            startIndex = Math.Min(startIndex, totalEntriesCount);
            endIndex = Math.Min(endIndex,     totalEntriesCount);
            var countToReturn = endIndex - startIndex;
            if (countToReturn == 0)
            {
                return new FactionListPage(Array.Empty<FactionListEntry>(),
                                           (ushort)totalEntriesCount);
            }

            // apply the sort method
            list.Sort(ServerGetSortMethod(order));

            // build the result array
            var result = new FactionListEntry[countToReturn];
            for (var index = 0; index < countToReturn; index++)
            {
                var faction = list[startIndex + index];
                result[index] = ServerCreateFactionListEntry(faction);
            }

            return new FactionListPage(result, (ushort)totalEntriesCount);
        }

        [RemoteCallSettings(timeInterval: 0.333, deliveryMode: DeliveryMode.ReliableSequenced)]
        private double ServerRemote_GetLastOnlineDate(string memberName)
        {
            var faction = ServerGetFaction(ServerRemoteContext.Character);
            if (SharedGetFactionKind(faction) == FactionKind.Public)
            {
                // not available for public factions
                throw new InvalidOperationException();
            }

            foreach (var entry in ServerGetFactionMembersReadOnly(faction))
            {
                if (entry.Name != memberName)
                {
                    continue;
                }

                var member = Server.Characters.GetPlayerCharacter(memberName);
                return PlayerCharacter.GetPrivateState(member).ServerLastActiveTime;
            }

            throw new Exception("Faction member not found: " + memberName);
        }

        /// <summary>
        /// Players can freely join only a public faction.
        /// </summary>
        private ApplicationCreateResult ServerRemote_JoinFaction(string clanTag)
        {
            var character = ServerRemoteContext.Character;

            if (ServerGetFaction(character) is not null)
            {
                // already has a faction - should be impossible
                return ApplicationCreateResult.Unknown;
            }

            if (NewbieProtectionSystem.SharedIsNewbie(character)
                || SharedIsUnderJoinCooldown(character))
            {
                // should be impossible as client performs the same check
                return ApplicationCreateResult.Unknown;
            }

            var faction = ServerGetFactionByClanTag(clanTag);
            var factionPublicState = Faction.GetPublicState(faction);
            if (factionPublicState.Kind != FactionKind.Public)
            {
                // should be impossible as client performs the same check
                throw new Exception("Not a public faction: " + faction);
            }

            if (Faction.GetPrivateState(faction).ServerPlayerLeaveDateDictionary
                       .ContainsKey(character.Id))
            {
                return ApplicationCreateResult.CannotRejoinInvitationRequired;
            }

            var maxMembers = FactionConstants.GetFactionMembersMax(FactionKind.Public);
            if (ServerGetFactionMembersReadOnly(faction).Count >= maxMembers)
            {
                return ApplicationCreateResult.ErrorFactionFull;
            }

            ServerApplications.RemoveAllApplicationsByApplicant(character);
            ServerInvitations.RemoveAllInvitationsFor(character);

            ServerAddMember(character, faction, FactionMemberRole.Member);
            ServerAddLogEntry(faction,
                              new FactionLogEntryMemberJoined(member: character,
                                                              approvedByOfficer: null));
            Logger.Important($"{character} joined {faction} (an public faction)");
            return ApplicationCreateResult.Success;
        }

        private void ServerRemote_LeaveFaction()
        {
            ServerLeaveFaction(ServerRemoteContext.Character);
        }

        [RemoteCallSettings(timeInterval: 0.333)]
        private void ServerRemote_UpgradeFactionLevel(byte toLevel)
        {
            var character = ServerRemoteContext.Character;
            var faction = ServerGetFaction(character);
            if (faction is null)
            {
                throw new Exception("Not a member of any faction");
            }

            var factionPublicState = Faction.GetPublicState(faction);
            if (toLevel != factionPublicState.Level + 1)
            {
                throw new Exception("Incorrect target upgrade level");
            }

            if (toLevel > FactionConstants.MaxFactionLevel)
            {
                throw new Exception("Cannot upgrade beyond max level");
            }

            var upgradeCost = FactionConstants.SharedGetFactionUpgradeCost(toLevel);
            var technologies = character.SharedGetTechnologies();
            if (technologies.LearningPoints < upgradeCost)
            {
                throw new Exception("Not enough learning points");
            }

            technologies.ServerRemoveLearningPoints(upgradeCost);

            factionPublicState.Level++;
            ServerAddLogEntry(faction, new FactionLogEntryFactionLevelUpgraded(character, toLevel));
            Logger.Info("Faction upgraded to level: " + factionPublicState.Level);
        }

        public readonly struct FactionListPage : IRemoteCallParameter
        {
            public readonly FactionListEntry[] Entries;

            public readonly ushort TotalEntriesCount;

            public FactionListPage(FactionListEntry[] entries, ushort totalEntriesCount)
            {
                this.Entries = entries;
                this.TotalEntriesCount = totalEntriesCount;
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;

                Refresh();

                static void Refresh()
                {
                    ClientSetCurrentFaction(null);

                    ClientCurrentReceivedInvitations.Clear();
                    ClientCurrentSubmittedApplications.Clear();

                    ClientCurrentFactionCreatedInvitations.Clear();
                    ClientCurrentFactionReceivedApplications.Clear();
                }
            }
        }
    }
}