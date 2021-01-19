namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class WorldObjectAccessModeSystem : ProtoSystem<WorldObjectAccessModeSystem>
    {
        public const string NotificationDontHaveAccess = "You don't have access!";

        public override string Name => "World object access system";

        public static void ClientSetDirectAccessMode(IStaticWorldObject worldObject, WorldObjectDirectAccessMode mode)
        {
            Instance.CallServer(_ => _.ServerRemote_SetDirectAccessMode(worldObject, mode));
        }

        public static void ClientSetFactionAccessMode(
            IStaticWorldObject worldObject,
            WorldObjectFactionAccessModes modes)
        {
            Instance.CallServer(_ => _.ServerRemote_SetFactionAccessMode(worldObject, modes));
        }

        public static bool ServerHasAccess(
            IStaticWorldObject worldObject,
            ICharacter character,
            bool writeToLog)
        {
            var privateState = worldObject.GetPrivateState<IObjectWithAccessModePrivateState>();
            return ServerHasAccess(worldObject,
                                   character,
                                   privateState.DirectAccessMode,
                                   privateState.FactionAccessMode,
                                   writeToLog);
        }

        public static bool ServerHasAccess(
            IStaticWorldObject worldObject,
            ICharacter character,
            WorldObjectDirectAccessMode directAccessMode,
            WorldObjectFactionAccessModes factionAccessModes,
            bool writeToLog)
        {
            if (LandClaimSystem.SharedIsWorldObjectOwnedByFaction(worldObject, out var clanTag))
            {
                if (ServerHasFactionAccess(character, clanTag, factionAccessModes))
                {
                    return true;
                }

                if (writeToLog)
                {
                    Logger.Warning(
                        $"Character cannot interact with {worldObject} - no access.",
                        character);

                    Instance.CallClient(
                        character,
                        _ => _.ClientRemote_OnCannotInteractNoAccess(worldObject));
                }

                return false;
            }

            switch (directAccessMode)
            {
                case WorldObjectDirectAccessMode.Closed:
                default:
                    if (writeToLog)
                    {
                        Logger.Warning(
                            $"Character cannot interact with {worldObject} - no access.",
                            character);

                        Instance.CallClient(
                            character,
                            _ => _.ClientRemote_OnCannotInteractNoAccess(worldObject));
                    }

                    return false;

                case WorldObjectDirectAccessMode.OpensToEveryone:
                    return true;

                case WorldObjectDirectAccessMode.OpensToObjectOwners:
                case WorldObjectDirectAccessMode.OpensToObjectOwnersOrAreaOwners:
                {
                    if (WorldObjectOwnersSystem.SharedIsOwner(character, worldObject))
                    {
                        // an object owner
                        return true;
                    }

                    // not an object owner
                    if (directAccessMode == WorldObjectDirectAccessMode.OpensToObjectOwnersOrAreaOwners)
                    {
                        if (LandClaimSystem.SharedIsOwnedLand(worldObject.TilePosition,
                                                              character,
                                                              requireFactionPermission: false,
                                                              out _))
                        {
                            // an area owner
                            return true;
                        }
                    }

                    if (CreativeModeSystem.SharedIsInCreativeMode(character))
                    {
                        return true;
                    }

                    // not an object owner and not an area owner
                    if (writeToLog)
                    {
                        WorldObjectOwnersSystem.ServerNotifyNotOwner(character,
                                                                     worldObject,
                                                                     isFactionAccess: false);
                    }

                    return false;
                }
            }
        }

        public static bool SharedHasAccess(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (IsClient)
            {
                // cannot perform this check on the client side
                return true;
            }

            return ServerHasAccess(worldObject, character, writeToLog);
        }

        private static bool ServerHasFactionAccess(
            ICharacter character,
            string objectClanTag,
            WorldObjectFactionAccessModes factionAccessModes)
        {
            if (factionAccessModes == WorldObjectFactionAccessModes.Closed)
            {
                // always closed
                return false;
            }

            if (factionAccessModes == WorldObjectFactionAccessModes.Everyone)
            {
                // always opened
                return true;
            }

            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            var playerClanTag = PlayerCharacter.GetPublicState(character).ClanTag;
            if (string.IsNullOrEmpty(playerClanTag))
            {
                // player don't have a faction
                return false;
            }

            if (factionAccessModes.HasFlag(WorldObjectFactionAccessModes.AllyFactionMembers))
            {
                if (objectClanTag == playerClanTag)
                {
                    // current faction also allowed
                    return true;
                }

                var objectFaction = FactionSystem.ServerGetFactionByClanTag(objectClanTag);
                return FactionSystem.SharedGetFactionDiplomacyStatus(objectFaction, playerClanTag)
                       == FactionDiplomacyStatus.Ally;
            }

            // all further checks are only for the current faction
            if (objectClanTag != playerClanTag)
            {
                return false;
            }

            if (factionAccessModes.HasFlag(WorldObjectFactionAccessModes.AllFactionMembers))
            {
                // all current faction members allowed
                return true;
            }

            // only a specific role or roles are allowed
            var characterRole = FactionSystem.ServerGetRole(character);
            return CheckAccessRights(WorldObjectFactionAccessModes.Leader,      FactionMemberRole.Leader)
                   || CheckAccessRights(WorldObjectFactionAccessModes.Officer1, FactionMemberRole.Officer1)
                   || CheckAccessRights(WorldObjectFactionAccessModes.Officer2, FactionMemberRole.Officer2)
                   || CheckAccessRights(WorldObjectFactionAccessModes.Officer3, FactionMemberRole.Officer3);

            bool CheckAccessRights(WorldObjectFactionAccessModes flag, FactionMemberRole role)
                => characterRole == role
                   && factionAccessModes.HasFlag(flag);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_OnCannotInteractNoAccess(IStaticWorldObject worldObject)
        {
            CannotInteractMessageDisplay.ClientOnCannotInteract(worldObject,
                                                                NotificationDontHaveAccess,
                                                                isOutOfRange: false);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 0.5, keyArgIndex: 0)]
        private void ServerRemote_SetDirectAccessMode(IStaticWorldObject worldObject, WorldObjectDirectAccessMode mode)
        {
            var character = ServerRemoteContext.Character;
            InteractionCheckerSystem.SharedValidateIsInteracting(character,
                                                                 worldObject,
                                                                 requirePrivateScope: true);

            if (!(worldObject.ProtoGameObject is IProtoObjectWithAccessMode protoObjectWithAccessMode))
            {
                throw new Exception("This world object doesn't have an access mode");
            }

            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(worldObject);
            if (areasGroup is not null
                && LandClaimAreasGroup.GetPublicState(areasGroup).ServerFaction is not null)
            {
                throw new Exception(
                    "Cannot modify direct access mode for an object within a faction land claim area");
            }

            if (!WorldObjectOwnersSystem.SharedIsOwner(character, worldObject)
                && !CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                throw new Exception("The player character is not an owner of " + worldObject);
            }

            if (mode == WorldObjectDirectAccessMode.Closed
                && !protoObjectWithAccessMode.IsClosedAccessModeAvailable)
            {
                throw new Exception("Closed access mode is not supported for " + protoObjectWithAccessMode);
            }

            var privateState = worldObject.GetPrivateState<IObjectWithAccessModePrivateState>();
            if (privateState.DirectAccessMode == mode)
            {
                return;
            }

            privateState.DirectAccessMode = mode;
            Logger.Info($"Direct access mode changed: {mode}; {worldObject}", character);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 0.5, keyArgIndex: 0)]
        private void ServerRemote_SetFactionAccessMode(
            IStaticWorldObject worldObject,
            WorldObjectFactionAccessModes modes)
        {
            var character = ServerRemoteContext.Character;
            InteractionCheckerSystem.SharedValidateIsInteracting(character,
                                                                 worldObject,
                                                                 requirePrivateScope: true);

            if (!(worldObject.ProtoGameObject is IProtoObjectWithAccessMode protoObjectWithAccessMode))
            {
                throw new Exception("This world object doesn't have an access mode");
            }

            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(worldObject);
            if (areasGroup is null)
            {
                throw new Exception(
                    "Cannot modify faction access mode for an object without a faction land claim area");
            }

            var faction = LandClaimAreasGroup.GetPublicState(areasGroup).ServerFaction;
            if (faction is null)
            {
                throw new Exception(
                    "Cannot modify faction access mode for an object without a faction land claim area");
            }

            if (!CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                // verify permission
                FactionSystem.ServerValidateHasAccessRights(character,
                                                            FactionMemberAccessRights.LandClaimManagement,
                                                            out var characterFaction);

                if (!ReferenceEquals(faction, characterFaction))
                {
                    throw new Exception(worldObject + " belongs to another faction - cannot modify its access");
                }
            }

            if (modes == WorldObjectFactionAccessModes.Closed
                && !protoObjectWithAccessMode.IsClosedAccessModeAvailable)
            {
                throw new Exception("Closed access mode is not supported for " + protoObjectWithAccessMode);
            }

            var privateState = worldObject.GetPrivateState<IObjectWithAccessModePrivateState>();
            if (privateState.FactionAccessMode == modes)
            {
                return;
            }

            privateState.FactionAccessMode = modes;
            Logger.Info($"Faction access mode changed: {modes}; {worldObject}", character);
        }
    }
}