namespace AtomicTorch.CBND.CoreMod.Systems.FactionAllyBaseRaidingNotificationSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class FactionAllyBaseRaidingNotificationSystem : ProtoSystem<FactionAllyBaseRaidingNotificationSystem>
    {
        public static readonly ObservableCollection<ClientAllyBaseUnderRaidMark> ClientAllyBasesUnderRaid
            = IsClient
                  ? new SuperObservableCollection<ClientAllyBaseUnderRaidMark>()
                  : null;

        private static readonly Dictionary<ServerAllyBaseUnderRaidMark, List<ICharacter>>
            ServerNotifiedCharactersForAreasGroups
                = IsServer
                      ? new Dictionary<ServerAllyBaseUnderRaidMark, List<ICharacter>>()
                      : null;

        protected override void PrepareSystem()
        {
            if (IsClient
                || PveSystem.ServerIsPvE)
            {
                return;
            }

            LandClaimSystem.ServerRaidBlockStartedOrExtended
                += ServerRaidBlockStartedOrExtendedHandler;

            Server.Characters.PlayerOnlineStateChanged
                += ServerPlayerOnlineStateChangedHandler;

            TriggerTimeInterval.ServerConfigureAndRegister(
                TimeSpan.FromSeconds(1),
                ServerUpdate,
                "System." + this.ShortId);
        }

        private static void ServerNotifyFactionAllies(
            ILogicObject faction,
            ILogicObject area,
            ILogicObject areasGroup)
        {
            var allyFactions = Faction.GetPrivateState(faction)
                                      .FactionDiplomacyStatuses
                                      .Where(p => p.Value == FactionDiplomacyStatus.Ally)
                                      .Select(p => FactionSystem.ServerGetFactionByClanTag(p.Key))
                                      .ToArray();

            if (allyFactions.Length == 0)
            {
                return;
            }

            var charactersToNotify = allyFactions.SelectMany(FactionSystem.ServerGetFactionMembersReadOnly)
                                                 .ToArray();

            var playerCharactersToNotify = new List<ICharacter>();
            foreach (var memberEntry in charactersToNotify)
            {
                var character = Server.Characters.GetPlayerCharacter(memberEntry.Name);
                if (character is not null
                    && !LandClaimSystem.ServerIsOwnedArea(area, character, requireFactionPermission: false))
                {
                    playerCharactersToNotify.Add(character);
                }
            }

            if (playerCharactersToNotify.Count == 0)
            {
                return;
            }

            var clanTag = FactionSystem.SharedGetClanTag(faction);
            var mark = new ServerAllyBaseUnderRaidMark(areasGroup, factionMemberName: null, clanTag);
            ServerNotifiedCharactersForAreasGroups[mark] = playerCharactersToNotify;

            var mapPosition = LandClaimSystem.SharedGetLandClaimGroupCenterPosition(areasGroup);
            Instance.CallClient(playerCharactersToNotify,
                                _ => _.ClientRemote_AllyBaseUnderRaid(
                                    areasGroup.Id,
                                    mark.FactionMemberName,
                                    mark.ClanTag,
                                    mapPosition));
        }

        private static void ServerNotifyFactionMembers(
            ILogicObject faction,
            ICharacter founderCharacter,
            ILogicObject area,
            ILogicObject areasGroup)
        {
            var charactersToNotify = FactionSystem.ServerGetFactionMembersReadOnly(faction);

            var playerCharactersToNotify = new List<ICharacter>();
            foreach (var memberEntry in charactersToNotify)
            {
                var character = Server.Characters.GetPlayerCharacter(memberEntry.Name);
                if (character is not null
                    && !LandClaimSystem.ServerIsOwnedArea(area, character, requireFactionPermission: false))
                {
                    playerCharactersToNotify.Add(character);
                }
            }

            if (playerCharactersToNotify.Count == 0)
            {
                return;
            }

            var founderCharacterName = founderCharacter.Name;
            var mark = new ServerAllyBaseUnderRaidMark(areasGroup, founderCharacterName, clanTag: null);
            ServerNotifiedCharactersForAreasGroups[mark] = playerCharactersToNotify;

            var mapPosition = LandClaimSystem.SharedGetLandClaimGroupCenterPosition(areasGroup);
            Instance.CallClient(playerCharactersToNotify,
                                _ => _.ClientRemote_AllyBaseUnderRaid(
                                    areasGroup.Id,
                                    mark.FactionMemberName,
                                    mark.ClanTag,
                                    mapPosition));
        }

        private static void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            if (!isOnline)
            {
                return;
            }

            // notify about the ongoing raids on ally bases
            foreach (var pair in ServerNotifiedCharactersForAreasGroups)
            {
                if (!pair.Value.Contains(character))
                {
                    // this character was not notified for that mark
                    continue;
                }

                var mark = pair.Key;
                var areasGroup = mark.AreasGroup;
                var mapPosition = LandClaimSystem.SharedGetLandClaimGroupCenterPosition(areasGroup);
                var areas = LandClaimAreasGroup.GetPrivateState(areasGroup)
                                               .ServerLandClaimsAreas;
                var faction = areas
                              .Select(LandClaimSystem.ServerGetLandOwnerFactionOrFounderFaction)
                              .FirstOrDefault(f => f is not null);
                if (faction is null)
                {
                    continue;
                }

                var isOwner = false;
                foreach (var area in areas)
                {
                    if (LandClaimSystem.ServerIsOwnedArea(area, character, requireFactionPermission: false))
                    {
                        isOwner = true;
                        break;
                    }
                }

                if (isOwner)
                {
                    // no need to notify
                    continue;
                }

                Instance.CallClient(character,
                                    _ => _.ClientRemote_AllyBaseUnderRaid(
                                        areasGroup.Id,
                                        mark.FactionMemberName,
                                        mark.ClanTag,
                                        mapPosition));
            }
        }

        private static void ServerRaidBlockStartedOrExtendedHandler(
            ILogicObject area,
            ICharacter raiderCharacter,
            bool isNewRaidBlock,
            bool isStructureDestroyed)
        {
            if (!isNewRaidBlock)
            {
                return;
            }

            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(area);
            if (ServerNotifiedCharactersForAreasGroups.ContainsKey(
                ServerAllyBaseUnderRaidMark.CreateKeyOnly(areasGroup)))
            {
                // notification for this areas group is already sent 
                return;
            }

            ILogicObject faction;
            var clanTag = LandClaimSystem.SharedGetAreaOwnerFactionClanTag(area);
            if (!string.IsNullOrEmpty(clanTag))
            {
                // owned by a faction, notify allies
                faction = FactionSystem.ServerGetFactionByClanTag(clanTag);
                ServerNotifyFactionAllies(faction, area, areasGroup);
                return;
            }

            // not owned by faction,
            // check whether its founder is a member of any faction and notify its members
            var founderName = LandClaimArea.GetPrivateState(area)
                                           .LandClaimFounder;
            var founderCharacter = Server.Characters.GetPlayerCharacter(founderName);
            if (founderCharacter is null)
            {
                return;
            }

            faction = FactionSystem.ServerGetFaction(founderCharacter);
            if (faction is not null)
            {
                ServerNotifyFactionMembers(faction, founderCharacter, area, areasGroup);
            }
        }

        private static void ServerUpdate()
        {
            ServerNotifiedCharactersForAreasGroups.ProcessAndRemove(
                removeCondition: key =>
                                 {
                                     // determine whether the raid has ended
                                     var areasGroup = key.AreasGroup;
                                     return areasGroup.IsDestroyed
                                            || !LandClaimSystem.SharedIsAreasGroupUnderRaid(areasGroup);
                                 },
                removeCallback: pair =>
                                {
                                    // notify all the previously notified players
                                    var previouslyNotifiedCharacters = pair.Value;
                                    Instance.CallClient(previouslyNotifiedCharacters,
                                                        _ => _.ClientRemote_AllyBaseRaidEnded(
                                                            pair.Key.AreasGroup.Id));
                                });
        }

        private void ClientRemote_AllyBaseRaidEnded(uint landClaimAreasGroupId)
        {
            var entry = new ClientAllyBaseUnderRaidMark(landClaimAreasGroupId);
            ClientAllyBasesUnderRaid.Remove(entry);
            //Logger.Dev("Ally base is no longer under attack: " + entry);
        }

        private void ClientRemote_AllyBaseUnderRaid(
            uint landClaimAreasGroupId,
            [CanBeNull] string factionMemberName,
            [CanBeNull] string clanTag,
            Vector2Ushort worldPosition)
        {
            var entry = new ClientAllyBaseUnderRaidMark(landClaimAreasGroupId,
                                                        factionMemberName,
                                                        clanTag,
                                                        worldPosition);
            ClientAllyBasesUnderRaid.Add(entry);
            //Logger.Dev("Ally base under attack: " + entry);
        }

        public readonly struct ClientAllyBaseUnderRaidMark : IEquatable<ClientAllyBaseUnderRaidMark>
        {
            /// <summary>
            /// Available only if base owner's name is null or empty.
            /// </summary>
            public readonly string ClanTag;

            /// <summary>
            /// Available only if clan tag is null or empty.
            /// </summary>
            public readonly string FactionMemberName;

            public readonly uint LandClaimAreasGroupId;

            public readonly Vector2Ushort WorldPosition;

            public ClientAllyBaseUnderRaidMark(
                uint landClaimAreasGroupId,
                string factionMemberName,
                string clanTag,
                Vector2Ushort worldPosition)
            {
                this.FactionMemberName = factionMemberName;
                this.ClanTag = clanTag;
                this.WorldPosition = worldPosition;
                this.LandClaimAreasGroupId = landClaimAreasGroupId;
            }

            public ClientAllyBaseUnderRaidMark(uint landClaimAreasGroupId)
                : this(landClaimAreasGroupId, null, null, Vector2Ushort.Zero)
            {
            }

            public bool Equals(ClientAllyBaseUnderRaidMark other)
            {
                return this.LandClaimAreasGroupId == other.LandClaimAreasGroupId;
            }

            public override bool Equals(object obj)
            {
                return obj is ClientAllyBaseUnderRaidMark other && this.Equals(other);
            }

            public override int GetHashCode()
            {
                return (int)this.LandClaimAreasGroupId;
            }
        }

        [NotPersistent]
        private readonly struct ServerAllyBaseUnderRaidMark : IEquatable<ServerAllyBaseUnderRaidMark>
        {
            public readonly ILogicObject AreasGroup;

            public readonly string ClanTag;

            public readonly string FactionMemberName;

            public ServerAllyBaseUnderRaidMark(ILogicObject areasGroup, string factionMemberName, string clanTag)
            {
                this.AreasGroup = areasGroup;
                this.FactionMemberName = factionMemberName;
                this.ClanTag = clanTag;
            }

            private ServerAllyBaseUnderRaidMark(ILogicObject areasGroup)
            {
                this.AreasGroup = areasGroup;
                this.FactionMemberName = null;
                this.ClanTag = null;
            }

            public static ServerAllyBaseUnderRaidMark CreateKeyOnly(ILogicObject areasGroup)
            {
                return new(areasGroup);
            }

            public bool Equals(ServerAllyBaseUnderRaidMark other)
            {
                return Equals(this.AreasGroup, other.AreasGroup);
            }

            public override bool Equals(object obj)
            {
                return obj is ServerAllyBaseUnderRaidMark other && this.Equals(other);
            }

            public override int GetHashCode()
            {
                return (this.AreasGroup != null ? this.AreasGroup.GetHashCode() : 0);
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
                    ClientAllyBasesUnderRaid.Clear();
                }
            }
        }
    }
}