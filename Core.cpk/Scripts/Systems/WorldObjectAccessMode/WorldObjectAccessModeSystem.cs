namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
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

        public static void ClientSetMode(IWorldObject worldObject, WorldObjectAccessMode mode)
        {
            Instance.CallServer(_ => _.ServerRemote_SetMode(worldObject, mode));
        }

        public static bool ServerHasAccess(
            IWorldObject worldObject,
            ICharacter character,
            bool writeToLog)
        {
            var privateState = worldObject.GetPrivateState<IObjectWithAccessModePrivateState>();
            return ServerHasAccess(worldObject,
                                   character,
                                   privateState.AccessMode,
                                   writeToLog);
        }

        public static bool ServerHasAccess(
            IWorldObject worldObject,
            ICharacter character,
            WorldObjectAccessMode currentAccessMode,
            bool writeToLog)
        {
            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            switch (currentAccessMode)
            {
                case WorldObjectAccessMode.Closed:
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

                case WorldObjectAccessMode.OpensToEveryone:
                    return true;

                case WorldObjectAccessMode.OpensToObjectOwners:
                case WorldObjectAccessMode.OpensToObjectOwnersOrAreaOwners:
                {
                    if (WorldObjectOwnersSystem.SharedIsOwner(character, worldObject))
                    {
                        // an object owner
                        return true;
                    }

                    // not an object owner
                    if (currentAccessMode == WorldObjectAccessMode.OpensToObjectOwnersOrAreaOwners)
                    {
                        if (LandClaimSystem.SharedIsOwnedLand(worldObject.TilePosition, character, out _))
                        {
                            // an area owner
                            return true;
                        }
                    }

                    // not an object owner and not an area owner
                    if (writeToLog)
                    {
                        WorldObjectOwnersSystem.ServerNotifyNotOwner(character, worldObject);
                    }

                    return false;
                }
            }
        }

        public static bool SharedHasAccess(ICharacter character, IWorldObject worldObject, bool writeToLog)
        {
            if (IsClient)
            {
                // cannot perform this check on the client side
                return true;
            }

            return ServerHasAccess(worldObject, character, writeToLog);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_OnCannotInteractNoAccess(IWorldObject worldObject)
        {
            CannotInteractMessageDisplay.ClientOnCannotInteract(worldObject,
                                                                NotificationDontHaveAccess,
                                                                isOutOfRange: false);
        }

        private void ServerRemote_SetMode(IWorldObject worldObject, WorldObjectAccessMode mode)
        {
            var character = ServerRemoteContext.Character;
            if (!InteractionCheckerSystem.SharedHasInteraction(character,
                                                               worldObject,
                                                               requirePrivateScope: true))
            {
                throw new Exception("The player character is not interacting with " + worldObject);
            }

            if (!WorldObjectOwnersSystem.SharedIsOwner(character, worldObject)
                && !CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                throw new Exception("The player character is not the owner of " + worldObject);
            }

            if (!(worldObject.ProtoGameObject is IProtoObjectWithAccessMode protoObjectWithAccessMode))
            {
                throw new Exception("This world object doesn't have an access mode");
            }

            if (mode == WorldObjectAccessMode.Closed
                && !protoObjectWithAccessMode.IsClosedAccessModeAvailable)
            {
                throw new Exception("Closed access mode is not supported for " + protoObjectWithAccessMode);
            }

            var privateState = worldObject.GetPrivateState<IObjectWithAccessModePrivateState>();
            privateState.AccessMode = mode;
            Logger.Important($"Access mode changed: {mode}; {worldObject}", character);
        }
    }
}