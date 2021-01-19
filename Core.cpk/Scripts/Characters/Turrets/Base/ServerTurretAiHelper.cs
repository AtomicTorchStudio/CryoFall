namespace AtomicTorch.CBND.CoreMod.Characters.Turrets
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    internal static class ServerTurretAiHelper
    {
        private static readonly IWorldServerService ServerWorldService
            = Api.IsServer
                  ? Api.Server.World
                  : null;

        private static readonly List<ICharacter> TempListCharactersInView
            = new();

        public static ICharacter GetClosestTargetPlayer(ICharacter characterTurret, TurretMode turretMode)
        {
            byte? tileHeight = null;

            try
            {
                var charactersInView = TempListCharactersInView;
                ServerWorldService.GetCharactersInView(characterTurret,
                                                       charactersInView,
                                                       onlyPlayerCharacters: true);
                if (charactersInView.Count == 0)
                {
                    return null;
                }

                var turretTilePosition = characterTurret.TilePosition;
                if (!GetTurretClaimData(turretTilePosition,
                                        out var landClaimAreasGroup,
                                        out var turretClanTag,
                                        out var turretFaction))
                {
                    return null;
                }

                ICharacter enemy = null;
                foreach (var target in charactersInView)
                {
                    if (!IsValidTargetInternal(characterTurret,
                                               target,
                                               turretClanTag,
                                               turretFaction,
                                               turretTilePosition,
                                               turretMode,
                                               landClaimAreasGroup,
                                               forceAttackNeutrals: false,
                                               ref tileHeight))
                    {
                        continue;
                    }

                    enemy = target;
                    break;
                }

                return enemy;
            }
            finally
            {
                TempListCharactersInView.Clear();
            }
        }

        public static bool IsValidTarget(
            ICharacter characterTurret,
            IWorldObject worldObject,
            TurretMode turretMode,
            bool forceAttackNeutrals)
        {
            if (worldObject is IDynamicWorldObject
                && worldObject.ProtoGameObject is IProtoVehicle)
            {
                worldObject = worldObject.GetPublicState<VehiclePublicState>().PilotCharacter;
                if (worldObject is null)
                {
                    // don't attack vehicles without pilots
                    // TODO: determine enemy vehicles and attack them
                    return false;
                }
            }

            if (worldObject is not ICharacter targetCharacter
                || targetCharacter.IsNpc)
            {
                return false;
            }

            var turretTilePosition = characterTurret.TilePosition;
            if (!GetTurretClaimData(turretTilePosition,
                                    out var landClaimAreasGroup,
                                    out var turretClanTag,
                                    out var turretFaction))
            {
                return false;
            }

            byte? tileHeight = null;
            return IsValidTargetInternal(characterTurret,
                                         targetCharacter,
                                         turretClanTag,
                                         turretFaction,
                                         turretTilePosition,
                                         turretMode,
                                         landClaimAreasGroup,
                                         forceAttackNeutrals,
                                         ref tileHeight);
        }

        private static bool GetTurretClaimData(
            Vector2Ushort turretTilePosition,
            out ILogicObject landClaimAreasGroup,
            out string turretClanTag,
            out ILogicObject turretFaction)
        {
            landClaimAreasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(turretTilePosition);
            if (landClaimAreasGroup is null)
            {
                // not inside a base
                turretClanTag = null;
                turretFaction = null;
                return false;
            }

            turretClanTag = LandClaimAreasGroup.GetPublicState(landClaimAreasGroup).FactionClanTag;
            if (string.IsNullOrEmpty(turretClanTag))
            {
                turretClanTag = null;
                turretFaction = null;
                var area = LandClaimAreasGroup.GetPrivateState(landClaimAreasGroup).ServerLandClaimsAreas[0];
                var founderName = LandClaimArea.GetPrivateState(area)
                                               .LandClaimFounder;
                var founderCharacter = Api.Server.Characters.GetPlayerCharacter(founderName);
                if (founderCharacter is not null)
                {
                    turretFaction = FactionSystem.ServerGetFaction(founderCharacter);
                    turretClanTag = FactionSystem.SharedGetClanTag(turretFaction);
                }
            }
            else
            {
                turretFaction = FactionSystem.ServerGetFactionByClanTag(turretClanTag);
            }

            return true;
        }

        private static bool IsValidTargetInternal(
            ICharacter characterTurret,
            ICharacter targetCharacter,
            string turretClanTag,
            ILogicObject turretFaction,
            Vector2Ushort turretTilePosition,
            TurretMode turretMode,
            ILogicObject landClaimAreasGroup,
            bool forceAttackNeutrals,
            ref byte? tileHeight)
        {
            if (targetCharacter.GetPublicState<ICharacterPublicState>().IsDead)
            {
                // do not pay attention to dead characters
                return false;
            }

            if (targetCharacter.IsNpc)
            {
                /*if (targetCharacter.ProtoGameObject is not IProtoCharacterMob)
                {
                    // allow attacking creatures // TODO: currently game doesn't support turret-to-creature damage
                    return false;
                }*/

                return false;
            }

            if (targetCharacter.ProtoGameObject.GetType() != typeof(PlayerCharacter))
            {
                // don't react on spectator and other special player character prototypes
                return false;
            }

            var diplomacyStatus = FactionDiplomacyStatus.Neutral;
            if (turretClanTag is not null)
            {
                var characterClanTag = PlayerCharacter.GetPublicState(targetCharacter).ClanTag;
                if (characterClanTag == turretClanTag)
                {
                    // don't attack a player character from the same faction as the turret's land claim
                    return false;
                }

                if (!string.IsNullOrEmpty(characterClanTag))
                {
                    diplomacyStatus = FactionSystem.SharedGetFactionDiplomacyStatus(turretFaction,
                        characterClanTag);

                    if (diplomacyStatus == FactionDiplomacyStatus.Ally)
                    {
                        // don't attack an ally
                        return false;
                    }
                }
            }
            else // non-faction land claim
            {
                if (LandClaimSystem.SharedIsOwnedLand(turretTilePosition,
                                                      targetCharacter,
                                                      requireFactionPermission: false,
                                                      out _))
                {
                    // don't attack land claim owner
                    return false;
                }
            }

            if (diplomacyStatus == FactionDiplomacyStatus.Neutral
                && !forceAttackNeutrals)
            {
                if (turretMode == TurretMode.AttackEnemiesAndTrespassers)
                {
                    // attack neutral only if trespasser or if hostile
                    if ((LandClaimSystem.SharedGetLandClaimAreasGroup(targetCharacter.TilePosition)
                         != landClaimAreasGroup)
                        && !ServerLandClaimAreasGroupHostileCharactersTracker.IsHostileCharacter(
                            targetCharacter,
                            landClaimAreasGroup))
                    {
                        // not a trespasser and non-hostile
                        return false;
                    }
                }
                else if (turretMode == TurretMode.AttackHostile
                         && !ServerLandClaimAreasGroupHostileCharactersTracker.IsHostileCharacter(
                             targetCharacter,
                             landClaimAreasGroup))
                {
                    // don't attack non-hostile players
                    return false;
                }
            }

            var targetTile = targetCharacter.Tile;
            if (!tileHeight.HasValue)
            {
                var npcTile = characterTurret.Tile;
                tileHeight = npcTile.Height;
            }

            if (targetTile.Height != tileHeight.Value)
            {
                // attack only on the same height characters
                // unless there is a direct line of sight between the NPC and the target
                if (ServerCharacterAiHelper.AnyObstaclesBetween(characterTurret, targetCharacter))
                {
                    return false;
                }
            }

            return true;
        }
    }
}