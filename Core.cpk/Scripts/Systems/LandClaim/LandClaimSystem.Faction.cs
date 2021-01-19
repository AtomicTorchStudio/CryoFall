namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public partial class LandClaimSystem
    {
        public static IEnumerable<ILogicObject> ClientEnumerateAllCurrentFactionAreas()
        {
            return SharedEnumerateAllFactionAreas(FactionSystem.ClientCurrentFactionClanTag);
        }

        public static void ClientTransferLandClaimToFactionOwnership(ILogicObject area)
        {
            var factionOwnedAreas = SharedEnumerateAllFactionAreas(FactionSystem.ClientCurrentFactionClanTag);
            var claimLimitRemains = FactionConstants.SharedGetFactionLandClaimsLimit(
                                        Faction.GetPublicState(FactionSystem.ClientCurrentFaction).Level)
                                    - factionOwnedAreas.Count();

            claimLimitRemains -= ClientGetKnownAreasForGroup(SharedGetLandClaimAreasGroup(area)).Count();

            if (claimLimitRemains < 0)
            {
                NotificationSystem.ClientShowNotification(
                    CoreStrings.WindowLandClaim_TransferToFactionOwnership,
                    CoreStrings.Faction_LandClaimNumberLimit_Reached
                    + "[br]"
                    + CoreStrings.Faction_LandClaimNumberLimit_CanIncrease,
                    NotificationColor.Bad);
                return;
            }

            Instance.CallServer(_ => _.ServerRemote_TransferLandClaimToFactionOwnership(area));
        }

        public static ILogicObject ServerGetLandOwnerFactionOrFounderFaction(ILogicObject area)
        {
            ILogicObject landOwnerFaction = null;
            {
                var landOwnerFactionClanTag = SharedGetAreaOwnerFactionClanTag(area);
                if (!string.IsNullOrEmpty(landOwnerFactionClanTag))
                {
                    landOwnerFaction = FactionSystem.ServerGetFactionByClanTag(landOwnerFactionClanTag);
                }
                else
                {
                    var founderName = LandClaimArea.GetPrivateState(area)
                                                   .LandClaimFounder;
                    var founderCharacter = Server.Characters.GetPlayerCharacter(founderName);
                    if (founderCharacter is not null)
                    {
                        landOwnerFaction = FactionSystem.ServerGetFaction(founderCharacter);
                    }
                }
            }
            return landOwnerFaction;
        }

        public static IEnumerable<ILogicObject> SharedEnumerateAllFactionAreas(string clanTag)
        {
            if (string.IsNullOrEmpty(clanTag))
            {
                throw new ArgumentNullException();
            }

            return SharedEnumerateAllAreas()
                .Where(a =>
                       {
                           var areasGroup = SharedGetLandClaimAreasGroup(a);
                           var areasGroupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
                           return areasGroupPublicState.FactionClanTag == clanTag;
                       });
        }

        [CanBeNull]
        public static string SharedGetAreaOwnerFactionClanTag(ILogicObject area)
        {
            var areasGroup = SharedGetLandClaimAreasGroup(area);
            var areasGroupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            return areasGroupPublicState.FactionClanTag;
        }

        public static bool SharedIsAreaOwnedByFaction(ILogicObject area)
        {
            return !string.IsNullOrEmpty(SharedGetAreaOwnerFactionClanTag(area));
        }

        public static bool SharedIsWorldObjectOwnedByFaction(IStaticWorldObject worldObject, out string clanTag)
        {
            var areasGroup = SharedGetLandClaimAreasGroup(worldObject);
            if (areasGroup is null)
            {
                clanTag = null;
                return false;
            }

            var areasGroupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            clanTag = areasGroupPublicState.FactionClanTag;
            if (string.IsNullOrEmpty(clanTag))
            {
                clanTag = null;
                return false;
            }

            return true;
        }

        public static bool SharedIsWorldObjectOwnedByFaction(IStaticWorldObject worldObject)
        {
            return SharedIsWorldObjectOwnedByFaction(worldObject, out _);
        }

        private static void ServerTransferAreasGroupToFactionOwnership(
            ILogicObject faction,
            ICharacter byCharacter,
            ILogicObject areasGroup)
        {
            var areas = LandClaimAreasGroup.GetPrivateState(areasGroup).ServerLandClaimsAreas;
            foreach (var area in areas)
            {
                ServerUnregisterArea(area);
            }

            var publicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            publicState.ServerSetFaction(faction);

            var centerTilePosition
                = new Vector2Ushort(
                    (ushort)areas.Average(a => LandClaimArea.GetPublicState(a).LandClaimCenterTilePosition.X),
                    (ushort)areas.Average(a => LandClaimArea.GetPublicState(a).LandClaimCenterTilePosition.Y));

            Logger.Important(
                $"Transferred land claim areas group to the faction ownership: {areasGroup} at {centerTilePosition}");
            FactionSystem.ServerOnLandClaimExpanded(faction, centerTilePosition, byCharacter);

            foreach (var area in areas)
            {
                ServerRegisterArea(area);
            }
        }

        [RemoteCallSettings(timeInterval: 5)]
        private void ServerRemote_TransferLandClaimToFactionOwnership(ILogicObject area)
        {
            var character = ServerRemoteContext.Character;
            var areasGroup = SharedGetLandClaimAreasGroup(area);

            if (LandClaimAreasGroup.GetPublicState(areasGroup).ServerFaction is not null)
            {
                // already has a faction (and it's not possible to change the faction)
                Logger.Warning("The land claim areas group is already transferred to a faction.");
                return;
            }

            FactionSystem.ServerValidateHasAccessRights(character,
                                                        FactionMemberAccessRights.LandClaimManagement,
                                                        out var faction);

            var factionOwnedAreas = SharedEnumerateAllFactionAreas(FactionSystem.SharedGetClanTag(faction));
            var claimLimitRemains = FactionConstants.SharedGetFactionLandClaimsLimit(
                                        Faction.GetPublicState(faction).Level)
                                    - factionOwnedAreas.Count();

            claimLimitRemains -= LandClaimAreasGroup.GetPrivateState(areasGroup).ServerLandClaimsAreas.Count;
            if (claimLimitRemains < 0)
            {
                Logger.Warning(
                    "Cannot transfer land claims to the faction as it will exceed the land claims number limit");
                return;
            }

            Logger.Important("Will transfer land claims to the faction: after upgrade the remaining limit will be "
                             + claimLimitRemains);

            // verify user has access to the land claim
            var owner = ServerRemoteContext.Character;
            if (!Server.World.IsInPrivateScope(area, owner))
            {
                throw new Exception(
                    "Cannot interact with the land claim object as the area is not in private scope: "
                    + area);
            }

            if (!LandClaimArea.GetPrivateState(area).ServerGetLandOwners()
                              .Contains(owner.Name))
            {
                throw new Exception("Player is not an owner of the land claim area");
            }

            ServerTransferAreasGroupToFactionOwnership(faction, character, areasGroup);

            var worldObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            InteractableWorldObjectHelper.ServerTryAbortInteraction(character, worldObject);
        }

        private class BootstrapperFactionWatcher : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                base.ServerInitialize(serverConfiguration);
                ServerAreasGroupChanged += ServerAreasGroupChangedHandler;
                FactionSystem.ServerFactionMemberAccessRightsChanged += ServerFactionMemberAccessRightsChangedHandler;
                FactionSystem.ServerCharacterJoinedOrLeftFaction += ServerCharacterJoinedOrLeftFactionHandler;
            }

            private static void ServerAreasGroupChangedHandler(
                ILogicObject area,
                [CanBeNull] ILogicObject areasGroupFrom,
                [CanBeNull] ILogicObject areasGroupTo)
            {
                if (areasGroupTo is not null)
                {
                    var clanTag = LandClaimAreasGroup.GetPublicState(areasGroupTo).FactionClanTag;
                    if (string.IsNullOrEmpty(clanTag))
                    {
                        return;
                    }

                    var faction = FactionSystem.ServerGetFactionByClanTag(clanTag);
                    var centerTilePosition = LandClaimArea.GetPublicState(area).LandClaimCenterTilePosition;
                    Logger.Important(
                        string.Format("Faction-owned land claim areas group expanded with a new claim area: {0} at {1}",
                                      areasGroupTo,
                                      centerTilePosition));

                    FactionSystem.ServerOnLandClaimExpanded(faction,
                                                            centerTilePosition,
                                                            byMember: ServerRemoteContext.IsRemoteCall
                                                                          ? ServerRemoteContext.Character
                                                                          : null);
                }
                else if (areasGroupFrom is not null)
                {
                    var clanTag = LandClaimAreasGroup.GetPublicState(areasGroupFrom).FactionClanTag;
                    if (string.IsNullOrEmpty(clanTag))
                    {
                        return;
                    }

                    var faction = FactionSystem.ServerGetFactionByClanTag(clanTag);
                    var centerTilePosition = LandClaimArea.GetPublicState(area).LandClaimCenterTilePosition;
                    Logger.Important(
                        string.Format("Faction-owned land claim areas group removed: {0} at {1}",
                                      areasGroupFrom,
                                      centerTilePosition));

                    FactionSystem.ServerOnLandClaimRemoved(faction,
                                                           centerTilePosition,
                                                           byMember: ServerRemoteContext.IsRemoteCall
                                                                         ? ServerRemoteContext.Character
                                                                         : null);
                }
            }

            private static void ServerCharacterJoinedOrLeftFactionHandler(
                ICharacter character,
                ILogicObject faction,
                bool isJoined)
            {
                if (isJoined)
                {
                    ServerRefreshFactionClaimAccess(faction);
                    return;
                }

                // left the faction - remove the faction claims from the character's private scope
                var ownedAreas = SharedGetPlayerOwnedAreas(character);
                using var tempList = Api.Shared.WrapInTempList(
                    (ICollection<ILogicObject>)ownedAreas);

                foreach (var area in tempList.AsList())
                {
                    var owners = area.GetPrivateState<LandClaimAreaPrivateState>().ServerGetLandOwners();
                    if (owners is null)
                    {
                        continue;
                    }

                    var isOwner = false;
                    foreach (var owner in owners)
                    {
                        if (character.Name != owner)
                        {
                            continue;
                        }

                        isOwner = true;
                        break;
                    }

                    if (isOwner)
                    {
                        continue;
                    }

                    ownedAreas.Remove(area);
                    if (character.ServerIsOnline)
                    {
                        ServerWorld.ExitPrivateScope(character, area);
                    }
                }
            }

            private static void ServerFactionMemberAccessRightsChangedHandler(
                ILogicObject faction,
                string characterName,
                FactionMemberAccessRights accessrights)
            {
                ServerRefreshFactionClaimAccess(faction);
            }

            private static void ServerRefreshFactionClaimAccess(ILogicObject faction)
            {
                using var tempList = Api.Shared.GetTempList<ILogicObject>();
                Api.GetProtoEntity<LandClaimAreasGroup>()
                   .GetAllGameObjects(tempList.AsList());

                foreach (var areasGroup in tempList.AsList())
                {
                    if (!ReferenceEquals(LandClaimAreasGroup.GetPublicState(areasGroup).ServerFaction,
                                         faction))
                    {
                        continue;
                    }

                    var areas = LandClaimAreasGroup.GetPrivateState(areasGroup)
                                                   .ServerLandClaimsAreas;
                    foreach (var area in areas)
                    {
                        ServerUnregisterArea(area);
                        ServerRegisterArea(area);
                    }
                }
            }
        }
    }
}