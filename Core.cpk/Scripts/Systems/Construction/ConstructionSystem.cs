namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConstructionSystem : ProtoSystem<ConstructionSystem>
    {
        public const string NotificationCannotPlace = "Cannot place";

        public const string NotificationNotEnoughItems_Message = "Need more items.";

        public const string NotificationNotEnoughItems_Title = "Cannot build";

        public override string Name => "Construction build/repair system";

        public static bool ClientTryAbortAction()
        {
            var privateState = ClientCurrentCharacterHelper.PrivateState;
            var actionState = privateState.CurrentActionState as ConstructionActionState;
            if (actionState == null)
            {
                return false;
            }

            // cancel repair/build state
            privateState.SetCurrentActionState(null);
            return true;
        }

        public static void ClientTryStartAction()
        {
            var worldObject = ClientWorldObjectInteractHelper.ClientFindWorldObjectAtCurrentMousePosition();
            if (!(worldObject?.ProtoStaticWorldObject is IProtoObjectStructure))
            {
                return;
            }

            var currentCharacter = Client.Characters.CurrentPlayerCharacter;
            var privateState = PlayerCharacter.GetPrivateState(currentCharacter);

            if (privateState.CurrentActionState is ConstructionActionState actionState
                && actionState.WorldObject == worldObject)
            {
                // the same object is already building/repairing
                return;
            }

            SharedStartAction(currentCharacter, worldObject);
        }

        public static IStaticWorldObject ServerCreateStructure(
            IProtoObjectStructure protoStructure,
            Vector2Ushort tilePosition,
            ICharacter byCharacter)
        {
            var structure = Api.Server.World.CreateStaticWorldObject(
                protoStructure,
                tilePosition);

            Logger.Important(byCharacter + " built " + structure);
            protoStructure.ServerOnBuilt(structure, byCharacter);
            return structure;
        }

        public static void SharedAbortAction(
            ICharacter character,
            IWorldObject worldObject)
        {
            if (worldObject == null)
            {
                return;
            }

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);

            var actionState = characterPrivateState.CurrentActionState as ConstructionActionState;
            if (actionState == null
                || actionState.WorldObject != worldObject)
            {
                // not repairing or repairing another object
                return;
            }

            if (!actionState.IsCompleted)
            {
                actionState.Cancel();
                return;
            }

            characterPrivateState.SetCurrentActionState(null);

            Logger.Info($"Building/repairing cancelled: {worldObject} by {character}", character);

            if (IsClient)
            {
                Instance.CallServer(_ => _.ServerRemote_Cancel(worldObject));
            }
            else // if Server
            {
                if (!ServerRemoteContext.IsRemoteCall)
                {
                    Instance.CallClient(character, _ => _.ClientRemote_Cancel(worldObject));
                    // TODO: notify other players as well
                }
            }
        }

        public static void SharedActionCompleted(ICharacter character, ConstructionActionState state)
        {
            var worldObject = state.WorldObject;

            Logger.Info($"Building/repairing completed: {worldObject} by {character}", character);

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            if (characterPrivateState.CurrentActionState != state)
            {
                throw new Exception("Should be impossible!");
            }

            characterPrivateState.SetCurrentActionState(null);
        }

        public static bool SharedCheckCanInteract(
            ICharacter character,
            IWorldObject worldObject,
            bool writeToLog)
        {
            if (worldObject == null
                || worldObject.IsDestroyed)
            {
                return false;
            }

            // Can build/repair only if the character can contact directly
            // with the object click area (if object has the click area)
            // or with the object collider (if object don't has the click area).
            var isObjectHasClickArea =
                worldObject.PhysicsBody.Shapes.Any(s => s.CollisionGroup == CollisionGroups.ClickArea);

            var result = worldObject.ProtoWorldObject.SharedIsInsideCharacterInteractionArea(
                character,
                worldObject,
                writeToLog: false,
                requiredCollisionGroup: isObjectHasClickArea
                                            ? CollisionGroups.ClickArea
                                            : CollisionGroups
                                                .DefaultWithCollisionToInteractionArea);
            if (!result)
            {
                if (writeToLog)
                {
                    Logger.Warning(
                        $"Cannot build/repair - {character} cannot interact with the {worldObject} - there is no direct (physics) line of sight between them (the object click area or static/dynamic colliders must be inside the character interaction area)");

                    if (IsClient)
                    {
                        CannotInteractMessageDisplay.ShowOn(worldObject, CoreStrings.Notification_TooFar);
                        worldObject.ProtoWorldObject.SharedGetObjectSoundPreset()
                                   .PlaySound(ObjectSound.InteractOutOfRange);
                    }
                }

                return false;
            }

            if (worldObject is IStaticWorldObject staticWorldObject)
            {
                // ensure the building is not in an area under the raid
                var world = IsClient
                                ? (IWorldService)Client.World
                                : Server.World;

                var protoStaticWorldObject = staticWorldObject.ProtoStaticWorldObject;
                var validatorNoRaid = LandClaimSystem.ValidatorNoRaid;
                foreach (var tileOffset in protoStaticWorldObject.Layout.TileOffsets)
                {
                    var occupiedTile = world.GetTile(
                        worldObject.TilePosition.X + tileOffset.X,
                        worldObject.TilePosition.Y + tileOffset.Y,
                        logOutOfBounds: false);

                    if (!occupiedTile.IsValidTile)
                    {
                        continue;
                    }

                    if (validatorNoRaid.Function(
                        new ConstructionTileRequirements.Context(
                            occupiedTile,
                            character,
                            protoStaticWorldObject,
                            tileOffset)))
                    {
                        continue;
                    }

                    // raid is under way - cannot build/repair
                    SharedShowCannotBuildNotification(
                        character,
                        validatorNoRaid.ErrorMessage,
                        protoStaticWorldObject);
                    return false;
                }
            }

            return true;
        }

        public static void SharedOnNotEnoughItemsAvailable(ConstructionActionState state)
        {
            if (Api.IsClient)
            {
                // show the notification directly
                Instance.ClientRemote_ShowNotEnoughItemsNotification(state.ProtoItemConstructionTool);
            }
            else
            {
                Instance.CallClient(
                    state.Character,
                    _ => _.ClientRemote_ShowNotEnoughItemsNotification(state.ProtoItemConstructionTool));
            }
        }

        public static void SharedShowCannotBuildNotification(
            ICharacter character,
            string errorMessage,
            IProtoStaticWorldObject proto)
        {
            if (IsClient)
            {
                Instance.ClientRemote_ClientShowNotificationCannotBuild(errorMessage, proto);
            }
            else
            {
                Instance.CallClient(
                    character,
                    _ => _.ClientRemote_ClientShowNotificationCannotBuild(errorMessage, proto));
            }
        }

        public static void SharedShowCannotPlaceNotification(
            ICharacter character,
            string errorMessage,
            IProtoStaticWorldObject proto)
        {
            if (IsClient)
            {
                Instance.ClientRemote_ClientShowNotificationCannotPlace(errorMessage, proto);
            }
            else
            {
                Instance.CallClient(
                    character,
                    _ => _.ClientRemote_ClientShowNotificationCannotPlace(errorMessage, proto));
            }
        }

        private static void SharedStartAction(ICharacter character, IWorldObject worldObject)
        {
            if (!(worldObject?.ProtoGameObject is IProtoObjectStructure))
            {
                return;
            }

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            var characterPublicState = PlayerCharacter.GetPublicState(character);

            if (characterPrivateState.CurrentActionState is ConstructionActionState actionState
                && actionState.WorldObject == worldObject)
            {
                // already building/repairing specified object
                return;
            }

            var selectedHotbarItem = characterPublicState.SelectedHotbarItem;
            if (!(selectedHotbarItem?.ProtoGameObject is IProtoItemToolToolbox))
            {
                // no tool is selected
                return;
            }

            actionState = new ConstructionActionState(character, (IStaticWorldObject)worldObject, selectedHotbarItem);
            if (!actionState.CheckIsNeeded())
            {
                // action is not needed
                Logger.Info($"Building/repairing is not required: {worldObject} by {character}", character);
                return;
            }

            if (!SharedCheckCanInteract(character, worldObject, writeToLog: true))
            {
                return;
            }

            if (!actionState.Config.IsAllowed)
            {
                // not allowed to build/repair
                // this code should never execute for a valid client
                Logger.Warning(
                    $"Building/repairing is not allowed by object build/repair config: {worldObject} by {character}",
                    character);

                if (Api.IsClient)
                {
                    NotificationSystem.ClientShowNotification(
                        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                        "Cannot construct",
                        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                        "Not constructable.",
                        NotificationColor.Bad,
                        selectedHotbarItem.ProtoItem.Icon);
                }

                return;
            }

            if (!actionState.ValidateRequiredItemsAvailable())
            {
                // action is not possible
                // logging is not required here - it's done automatically by the validation method
                return;
            }

            characterPrivateState.SetCurrentActionState(actionState);

            Logger.Info($"Building/repairing started: {worldObject} by {character}", character);

            if (IsClient)
            {
                // TODO: we need animation for building/repairing
                Instance.CallServer(_ => _.ServerRemote_StartAction(worldObject));
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_Cancel(IWorldObject worldObject)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            var characterPrivateState = PlayerCharacter.GetPrivateState(character);

            var actionState = characterPrivateState.CurrentActionState as ConstructionActionState;
            if (actionState == null
                || actionState.WorldObject != worldObject)
            {
                // not repairing or repairing another object
                return;
            }

            // cancel action
            characterPrivateState.SetCurrentActionState(null);
        }

        private void ClientRemote_ClientShowNotificationCannotBuild(string errorMessage, IProtoStaticWorldObject proto)
        {
            NotificationSystem.ClientShowNotification(
                NotificationCannotPlace,
                errorMessage,
                color: NotificationColor.Bad,
                icon: proto.Icon);
        }

        private void ClientRemote_ClientShowNotificationCannotPlace(string errorMessage, IProtoStaticWorldObject proto)
        {
            NotificationSystem.ClientShowNotification(
                title: null,
                errorMessage,
                color: NotificationColor.Bad,
                icon: proto.Icon);
        }

        private void ClientRemote_ShowNotEnoughItemsNotification(IProtoItemToolToolbox protoItemToolToolbox)
        {
            NotificationSystem.ClientShowNotification(
                NotificationNotEnoughItems_Title,
                NotificationNotEnoughItems_Message,
                NotificationColor.Bad,
                protoItemToolToolbox.Icon,
                playSound: false);
            ObjectsSoundsPresets.ObjectConstructionSite.PlaySound(ObjectSound.InteractFail);
        }

        private void ServerRemote_Cancel(IWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;
            SharedAbortAction(character, worldObject);
        }

        private void ServerRemote_StartAction(IWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;
            SharedStartAction(character, worldObject);
        }
    }
}