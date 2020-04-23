namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConstructionSystem : ProtoSystem<ConstructionSystem>
    {
        public const string NotificationCannotPlace = "Cannot place";

        public const string NotificationNotEnoughItems_Message = "Need more items.";

        public const string NotificationNotEnoughItems_Title = "Cannot build";

        private const double MaxDistanceForBuildRepairAction = 2.24;

        public override string Name => "Construction build/repair system";

        public static bool CheckCanInteractForConstruction(
            ICharacter character,
            IStaticWorldObject worldObject,
            bool writeToLog,
            bool checkRaidblock)
        {
            var characterPosition = character.Position;
            var canInteract = false;
            var staticWorldObjectProto = ProtoObjectConstructionSite.SharedGetConstructionProto(worldObject)
                                         ?? worldObject.ProtoStaticWorldObject;

            var startTilePosition = worldObject.TilePosition;
            foreach (var tileOffset in staticWorldObjectProto.Layout.TileOffsets)
            {
                var tilePosition = startTilePosition + tileOffset;

                if (characterPosition.DistanceSquaredTo(
                        new Vector2D(tilePosition.X + 0.5, tilePosition.Y + 0.5))
                    <= MaxDistanceForBuildRepairAction * MaxDistanceForBuildRepairAction)
                {
                    canInteract = true;
                    break;
                }
            }

            if (!canInteract)
            {
                canInteract = CreativeModeSystem.SharedIsInCreativeMode(character);
            }

            if (!canInteract)
            {
                if (writeToLog)
                {
                    Logger.Warning(
                        $"Character cannot interact with {worldObject} for (de)construction - too far.",
                        character);

                    if (IsClient)
                    {
                        CannotInteractMessageDisplay.ClientOnCannotInteract(worldObject,
                                                                            CoreStrings.Notification_TooFar,
                                                                            isOutOfRange: true);
                    }
                }

                return false;
            }

            if (checkRaidblock
                && LandClaimSystem.SharedIsUnderRaidBlock(character, worldObject))
            {
                // the building is in an area under the raid
                if (writeToLog)
                {
                    LandClaimSystem.SharedSendNotificationActionForbiddenUnderRaidblock(character);
                }

                return false;
            }

            return true;
        }

        public static IWorldObject ClientFindWorldObjectAtCurrentMousePosition()
        {
            var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            var objects = ClientComponentObjectInteractionHelper
                          // find by click area
                          .FindObjectsAtCurrentMousePosition(
                              currentCharacter,
                              CollisionGroups.ClickArea)
                          // find by default collider
                          .Concat(
                              ClientComponentObjectInteractionHelper
                                  .FindObjectsAtCurrentMousePosition(
                                      currentCharacter,
                                      CollisionGroups.Default))
                          //find object in the pointed tile
                          .Concat(
                              Api.Client.World.GetTile(Api.Client.Input.MouseWorldPosition.ToVector2Ushort())
                                 .StaticObjects.OrderByDescending(o => o.ProtoStaticWorldObject.Kind));

            // find first damaged or incomplete structure there
            foreach (var worldObject in objects)
            {
                if (!(worldObject.ProtoGameObject is IProtoObjectStructure protoObjectStructure))
                {
                    continue;
                }

                var maxStructurePointsMax =
                    protoObjectStructure.SharedGetStructurePointsMax((IStaticWorldObject)worldObject);
                var structurePointsCurrent =
                    worldObject.GetPublicState<StaticObjectPublicState>().StructurePointsCurrent;
                if (structurePointsCurrent < maxStructurePointsMax)
                {
                    return worldObject;
                }
            }

            return null;
        }

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
            var worldObject = ClientFindWorldObjectAtCurrentMousePosition();
            if (!(worldObject?.ProtoGameObject is IProtoObjectStructure))
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

            if (IsClient
                && (worldObject.IsDestroyed
                    || actionState.ObjectPublicState.StructurePointsCurrent >= actionState.StructurePointsMax))
            {
                // apparently the building finished construction/repair before the client simulation was complete
                SharedActionCompleted(character, actionState);
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

            if (IsClient)
            {
                // play success sound
                Api.GetProtoEntity<ObjectConstructionSite>()
                   .SharedGetObjectSoundPreset()
                   .PlaySound(ObjectSound.InteractSuccess,
                              limitOnePerFrame: false,
                              volume: 0.5f);
            }
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

            // it's possible to build/repair any building within a certain distance to the character
            var staticWorldObject = (IStaticWorldObject)worldObject;
            var canInteract = CheckCanInteractForConstruction(character,
                                                              staticWorldObject,
                                                              writeToLog,
                                                              checkRaidblock: false);
            if (!canInteract)
            {
                return false;
            }

            if (LandClaimSystem.SharedIsUnderRaidBlock(character, staticWorldObject))
            {
                // the building is in an area under the raid
                SharedShowCannotBuildNotification(
                    character,
                    LandClaimSystem.ErrorCannotBuild_RaidUnderWay,
                    staticWorldObject.ProtoStaticWorldObject);
                return false;
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

            var selectedHotbarItem = characterPublicState.SelectedItem;
            if (!(selectedHotbarItem?.ProtoGameObject is IProtoItemToolToolbox))
            {
                // no tool is selected
                return;
            }

            actionState = new ConstructionActionState(character, (IStaticWorldObject)worldObject, selectedHotbarItem);
            if (!actionState.CheckIsAllowed())
            {
                // not allowed to construct - currently it's possible only due to PvE limitation
                Logger.Warning(
                    $"Construction is not allowed: {worldObject} by {character}",
                    character);

                if (Api.IsClient)
                {
                    NotificationSystem.ClientShowNotification(
                        title: PveSystem.Notification_StuffBelongsToAnotherPlayer_Message,
                        LandClaimSystem.ErrorNotLandOwner_Message,
                        NotificationColor.Bad,
                        selectedHotbarItem?.ProtoItem.Icon);
                }

                return;
            }

            if (!actionState.CheckIsNeeded())
            {
                // action is not needed
                Logger.Info($"Building/repairing is not required: {worldObject} by {character}", character);
                return;
            }

            if (!SharedCheckCanInteract(character, worldObject, true))
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
                // "Cannot build"
                NotificationNotEnoughItems_Title,
                errorMessage,
                NotificationColor.Bad,
                proto.Icon);
        }

        private void ClientRemote_ClientShowNotificationCannotPlace(string errorMessage, IProtoStaticWorldObject proto)
        {
            NotificationSystem.ClientShowNotification(
                // "Cannot place"
                NotificationCannotPlace,
                errorMessage,
                NotificationColor.Bad,
                proto.Icon);
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