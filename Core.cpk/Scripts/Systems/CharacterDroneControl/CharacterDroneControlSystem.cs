namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDroneControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Drones;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class CharacterDroneControlSystem : ProtoSystem<CharacterDroneControlSystem>
    {
        /// <summary>
        /// If player walks away beyond this distance from its drone, it's considered abandoned,
        /// the control is lost and the drone drops on the ground.
        /// </summary>
        public const double DroneAbandonedDistanceMax = 33;

        /// <summary>
        /// Cannot walk away from drone on more tiles than this constant. It will start returning.
        /// </summary>
        public const double DroneOperationDistanceMax = 15;

        /// <summary>
        /// Cannot start a drone for an object located more than this distance away.
        /// </summary>
        public const double DroneStartDistanceMax = 10;

        public const string Notification_CannotMineThat =
            "Cannot mine that!";

        public const string Notification_DroneAlreadySent =
            "Already sent a drone there!";

        public const string Notification_DroneDeactivated_Message =
            @"The drone cannot return to you and was deactivated.
              [br]You can find it on the ground in its last location.";

        public const string Notification_DroneDeactivated_Title = "Drone deactivated";

        public const string Notification_DroneDestroyed_Title = "Drone destroyed";

        public const string Notification_ErrorCannotControlMoreDrones_Message_Format =
            "You cannot control more than {0} drones simultaneously with this device.";

        public const string Notification_ErrorCannotControlMoreDrones_Title =
            "Cannot control more drones";

        public const string Notification_ErrorNoDrones_Title =
            "No drones to launch!";

        public const string Notification_NothingToMineThere =
            "Nothing to mine there!";

        private static readonly ListDictionary<IItem, Vector2Ushort> ClientDroneStartQueue
            = new();

        private static readonly Dictionary<Vector2Ushort, IDynamicWorldObject> ServerCurrentMinedObjectsByDrones
            = IsServer
                  ? new Dictionary<Vector2Ushort, IDynamicWorldObject>()
                  : null;

        [NotLocalizable]
        public override string Name => "Character drone control system";

        public static IItem ClientSelectNextDrone(List<IItem> exceptItems)
        {
            IItem selectedItem = null;
            var selectedItemOrder = int.MinValue;
            var privateState = ClientCurrentCharacterHelper.PrivateState;

            // find a drone item with the highest order number
            foreach (var item in privateState.ContainerHotbar.Items)
            {
                if (IsValidDroneItem(item, out var order)
                    && order > selectedItemOrder)
                {
                    selectedItem = item;
                    selectedItemOrder = order;
                }
            }

            foreach (var item in privateState.ContainerInventory.Items)
            {
                if (IsValidDroneItem(item, out var order)
                    && order > selectedItemOrder)
                {
                    selectedItem = item;
                    selectedItemOrder = order;
                }
            }

            return selectedItem;

            bool IsValidDroneItem(IItem item, out int order)
            {
                if (item.ProtoItem is IProtoItemDrone protoItemDrone
                    && ItemDurabilitySystem.SharedGetDurabilityValue(item) > 0
                    && !exceptItems.Contains(item))
                {
                    order = protoItemDrone.SelectionOrder;
                    return true;
                }

                order = int.MinValue;
                return false;
            }
        }

        public static void ClientSubmitStartDroneCommandsImmediately()
        {
            switch (ClientDroneStartQueue.Count)
            {
                case 0:
                    return;

                case 1:
                    var onlyEntry = ClientDroneStartQueue.GetFirstEntry();
                    Instance.CallServer(_ => _.ServerRemote_StartDrone(onlyEntry.Key, onlyEntry.Value));
                    ClientDroneStartQueue.Clear();
                    return;

                default:
                {
                    using var tempList = Api.Shared.GetTempList<(IItem, Vector2Ushort)>();
                    foreach (var entry in ClientDroneStartQueue)
                    {
                        tempList.Add((entry.Key, entry.Value));
                    }

                    ClientDroneStartQueue.Clear();
                    // ReSharper disable once AccessToDisposedClosure
                    Instance.CallServer(_ => _.ServerRemote_StartDrones(tempList.AsList()));
                    return;
                }
            }
        }

        public static bool ClientTryStartDrone(
            IItem itemDrone,
            Vector2Ushort worldPosition,
            bool showErrorNotification)
        {
            var character = ClientCurrentCharacterHelper.Character;
            if (SharedIsMaxDronesToControlNumberExceeded(character,
                                                         showErrorNotification)
                || SharedIsTargetAlreadyScheduledForAnyActiveDrone(character,
                                                                   worldPosition,
                                                                   showErrorNotification))
            {
                return false;
            }

            var targetObject = SharedGetCompatibleTarget(character,
                                                         worldPosition,
                                                         out var hasIncompatibleTarget,
                                                         out var isPveActionForbidden);
            if (targetObject is null)
            {
                // nothing to mine there
                if (showErrorNotification)
                {
                    if (isPveActionForbidden)
                    {
                        PveSystem.ClientShowNotificationActionForbidden();
                    }

                    CannotInteractMessageDisplay.ClientOnCannotInteract(
                        character,
                        hasIncompatibleTarget
                            ? Notification_CannotMineThat
                            : Notification_NothingToMineThere,
                        isOutOfRange: false);
                }

                return false;
            }

            if (!SharedIsValidStartLocation(character, worldPosition, out var hasObstacles))
            {
                if (showErrorNotification)
                {
                    CannotInteractMessageDisplay.ClientOnCannotInteract(
                        character,
                        hasObstacles
                            ? CoreStrings.Notification_ObstaclesOnTheWay
                            : CoreStrings.Notification_TooFar,
                        isOutOfRange: true);
                }

                return false;
            }

            ClientDroneStartQueue.Add(itemDrone, worldPosition);
            return true;
        }

        public static ILogicObject ServerCreateCharacterDroneController()
        {
            return Server.World.CreateLogicObject<CharacterDroneController>();
        }

        public static void ServerDeactivateDrone(IDynamicWorldObject objectDrone)
        {
            var privateState = objectDrone.GetPrivateState<DronePrivateState>();
            var characterOwner = privateState.CharacterOwner;
            var protoItemDrone = (IProtoItemDrone)privateState.AssociatedItem.ProtoItem;
            var protoDrone = (IProtoDrone)objectDrone.ProtoGameObject;

            protoDrone.ServerDropDroneToGround(objectDrone, objectDrone.Tile, characterOwner);

            Logger.Info("Drone deactivated: " + objectDrone);
            ServerDespawnDrone(objectDrone, isReturnedToPlayer: false);

            Instance.CallClient(characterOwner,
                                _ => _.ClientRemote_OnDroneAbandoned(protoItemDrone));
        }

        public static void ServerDespawnDrone(IDynamicWorldObject objectDrone, bool isReturnedToPlayer)
        {
            if (objectDrone.IsDestroyed)
            {
                return;
            }

            var privateState = objectDrone.GetPrivateState<DronePrivateState>();
            var publicState = objectDrone.GetPublicState<DronePublicState>();

            publicState.ResetTargetPosition();
            if (privateState.IsDespawned)
            {
                return;
            }

            var droneItem = privateState.AssociatedItem;
            var protoItemDrone = (IProtoItemDrone)droneItem.ProtoItem;
            var characterOwner = privateState.CharacterOwner;
            var world = Server.World;

            ServerOnDroneControlRemoved(characterOwner, objectDrone);

            var protoDrone = protoItemDrone.ProtoDrone;
            protoDrone.ServerOnDroneDroppedOrReturned(objectDrone, characterOwner, isReturnedToPlayer);
            privateState.IsDespawned = true;
            privateState.CharacterOwner = null;

            // recreate physics (as despawned drone doesn't have any physics)
            world.StopPhysicsBody(objectDrone.PhysicsBody);
            objectDrone.ProtoWorldObject.SharedCreatePhysics(objectDrone);
            world.SetPosition(objectDrone,
                              CharacterDespawnSystem.ServerGetServiceAreaPosition().ToVector2D());

            try
            {
                DeductDurability();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            void DeductDurability()
            {
                var currentDurability = (int)(objectDrone.GetPublicState<DronePublicState>().StructurePointsCurrent
                                              / protoItemDrone.DurabilityToStructurePointsConversionCoefficient);
                if (currentDurability <= 1)
                {
                    currentDurability = 0;
                }

                var deltaDurabilility = (int)(ItemDurabilitySystem.SharedGetDurabilityValue(droneItem)
                                              - currentDurability);

                if (deltaDurabilility <= 0)
                {
                    return;
                }

                ItemDurabilitySystem.ServerModifyDurability(droneItem,
                                                            -deltaDurabilility);

                if (!droneItem.IsDestroyed)
                {
                    return;
                }

                // drone item degraded to 100%, notify the player
                ItemDurabilitySystem.Instance.CallClient(characterOwner,
                                                         _ => _.ClientRemote_ItemBroke(droneItem.ProtoItem));
                Server.World.DestroyObject(objectDrone);
            }
        }

        public static bool ServerIsMiningAllowed(Vector2Ushort tilePosition, IDynamicWorldObject droneObject)
        {
            if (ServerCurrentMinedObjectsByDrones.TryGetValue(tilePosition, out var existingDroneObject))
            {
                return ReferenceEquals(existingDroneObject, droneObject);
            }

            return true;
        }

        public static void ServerNotifyDroneAbandoned(ICharacter characterOwner, IProtoItemDrone protoItemDrone)
        {
            Instance.CallClient(characterOwner,
                                _ => _.ClientRemote_OnDroneAbandoned(protoItemDrone));
        }

        public static void ServerOnDroneControlRemoved(ICharacter character, IDynamicWorldObject objectDrone)
        {
            if (character is null)
            {
                return;
            }

            var controlledDrones = character.SharedGetCurrentlyControlledDrones();
            if (controlledDrones.Remove(objectDrone))
            {
                Logger.Info("Drone control ended: " + objectDrone);
            }
        }

        public static void ServerOnDroneDestroyed(IDynamicWorldObject objectDrone)
        {
            Logger.Info("Drone destroyed: " + objectDrone);
            var droneTile = objectDrone.Tile;

            var privateState = objectDrone.GetPrivateState<DronePrivateState>();
            var characterOwner = privateState.CharacterOwner;

            ServerUnregisterCurrentMining(objectDrone);
            ServerOnDroneControlRemoved(characterOwner, objectDrone);

            var droneItem = privateState.AssociatedItem;
            if (droneItem is not null)
            {
                Server.Items.DestroyItem(droneItem);
            }

            if (privateState.AssociatedItemReservedSlot is not null)
            {
                Server.Items.DestroyItem(privateState.AssociatedItemReservedSlot);
            }

            try
            {
                var protoDrone = (IProtoDrone)objectDrone.ProtoGameObject;
                protoDrone.ServerDropDroneToGround(objectDrone, droneTile, characterOwner);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            ServerTimersSystem.AddAction(
                0,
                () =>
                {
                    Server.Items.DestroyContainer(privateState.ReservedItemsContainer);
                    Server.Items.DestroyContainer(privateState.StorageItemsContainer);
                });

            if (droneItem is null
                || characterOwner is null)
            {
                return;
            }

            var protoItemDrone = (IProtoItemDrone)droneItem.ProtoItem;
            Instance.CallClient(characterOwner,
                                _ => _.ClientRemote_OnDroneDestroyed(protoItemDrone));
        }

        public static void ServerRecallAllDrones(ICharacter character)
        {
            if (character is null)
            {
                return;
            }

            var drones = character.SharedGetCurrentlyControlledDrones();
            if (drones.Count == 0)
            {
                return;
            }

            foreach (var objectDrone in drones.ToList())
            {
                ServerRecallDrone(objectDrone);
            }
        }

        public static void ServerRecallDrone(IDynamicWorldObject objectDrone)
        {
            objectDrone.GetPublicState<DronePublicState>()
                       .ResetTargetPosition();
            ServerUnregisterCurrentMining(objectDrone);
        }

        public static bool ServerTryRegisterCurrentMining(
            Vector2Ushort tilePosition,
            IDynamicWorldObject droneObject)
        {
            if (ServerCurrentMinedObjectsByDrones.TryGetValue(tilePosition, out var existingDroneObject))
            {
                return ReferenceEquals(existingDroneObject, droneObject);
            }

            ServerCurrentMinedObjectsByDrones.Add(tilePosition, droneObject);
            return true;
        }

        public static void ServerUnregisterCurrentMining(Vector2Ushort tilePosition, IDynamicWorldObject droneObject)
        {
            if (ServerCurrentMinedObjectsByDrones.TryGetValue(tilePosition, out var existingDroneObject)
                && ReferenceEquals(existingDroneObject, droneObject))
            {
                ServerCurrentMinedObjectsByDrones.Remove(tilePosition);
            }
        }

        public static void ServerUnregisterCurrentMining(IDynamicWorldObject droneObject)
        {
            ServerCurrentMinedObjectsByDrones.RemoveAllByValue(
                existingDroneObject =>
                    ReferenceEquals(existingDroneObject, droneObject));
        }

        public static IStaticWorldObject SharedGetCompatibleTarget(
            ICharacter character,
            Vector2Ushort worldPosition,
            out bool hasIncompatibleTarget,
            out bool isPveActionForbidden)
        {
            isPveActionForbidden = false;
            var targetObject = SharedGetCompatibleTargetObject(worldPosition);
            if (targetObject is null)
            {
                hasIncompatibleTarget = false;
                return null;
            }

            if (!PveSystem.SharedIsAllowStaticObjectDamage(character, targetObject, showClientNotification: false))
            {
                hasIncompatibleTarget = true;
                isPveActionForbidden = true;
                return null;
            }

            hasIncompatibleTarget = false;
            return targetObject;
        }

        public static IStaticWorldObject SharedGetCompatibleTargetObject(Vector2Ushort worldPosition)
        {
            var tile = IsServer
                           ? Server.World.GetTile(worldPosition)
                           : Client.World.GetTile(worldPosition);

            return SharedGetCompatibleTargetObject(tile);
        }

        public static IStaticWorldObject SharedGetCompatibleTargetObject(Tile tile)
        {
            IStaticWorldObject targetObject = null;
            var tileStaticObjects = tile.StaticObjects;
            for (var index = tileStaticObjects.Count - 1; index >= 0; index--)
            {
                var worldObject = tileStaticObjects[index];
                if (SharedIsValidDroneTarget(worldObject))
                {
                    targetObject = worldObject;
                    break;
                }
            }

            return targetObject;
        }

        public static bool SharedIsBeyondDroneAbandonDistance(
            Vector2Ushort characterPosition,
            Vector2Ushort dronePosition)
        {
            return characterPosition.TileSqrDistanceTo(dronePosition)
                   > DroneAbandonedDistanceMax * DroneAbandonedDistanceMax;
        }

        public static bool SharedIsMaxDronesToControlNumberExceeded(
            ICharacter character,
            bool clientShowErrorNotification = true)
        {
            if (character.SharedGetPlayerSelectedHotbarItemProto()
                    is not IProtoItemDroneControl protoItemControl)
            {
                return true;
            }

            var controlledDronesNumber = character.SharedGetCurrentControlledDronesNumber();
            if (controlledDronesNumber < protoItemControl.MaxDronesToControl)
            {
                return false;
            }

            if (IsClient && clientShowErrorNotification)
            {
                NotificationSystem.ClientShowNotification(
                    Notification_ErrorCannotControlMoreDrones_Title,
                    string.Format(Notification_ErrorCannotControlMoreDrones_Message_Format,
                                  protoItemControl.MaxDronesToControl),
                    color: NotificationColor.Neutral,
                    icon: protoItemControl.Icon);
            }

            return true;
        }

        public static bool SharedIsTargetAlreadyScheduledForAnyActiveDrone(
            ICharacter character,
            Vector2Ushort worldPosition,
            bool logError)
        {
            foreach (var objectDrone in character.SharedGetCurrentlyControlledDrones())
            {
                var droneTargetPosition = objectDrone.GetPublicState<DronePublicState>()
                                                     .TargetObjectPosition;
                if (droneTargetPosition != worldPosition)
                {
                    continue;
                }

                // the drone is already sent there
                if (logError)
                {
                    if (IsServer)
                    {
                        Logger.Info("Drone already sent there: " + worldPosition, character);
                    }
                    else
                    {
                        CannotInteractMessageDisplay.ClientOnCannotInteract(ClientCurrentCharacterHelper.Character,
                                                                            Notification_DroneAlreadySent,
                                                                            isOutOfRange: false);
                    }
                }

                return true;
            }

            return false;
        }

        public static bool SharedIsValidDroneOperationDistance(
            Vector2Ushort characterPosition,
            Vector2Ushort dronePosition)
        {
            return characterPosition.TileSqrDistanceTo(dronePosition)
                   <= DroneOperationDistanceMax * DroneOperationDistanceMax;
        }

        public static bool SharedIsValidDroneTarget(IStaticWorldObject worldObject)
        {
            return worldObject?.ProtoGameObject switch
            {
                IProtoObjectTree                                 => true,
                IProtoObjectMineral { IsAllowDroneMining: true } => true,
                _                                                => false
            };
        }

        /// <summary>
        /// Perform distance, height, and other checks.
        /// </summary>
        public static bool SharedIsValidStartLocation(
            ICharacter character,
            Vector2Ushort targetPosition,
            out bool hasObstacles)
        {
            hasObstacles = false;
            if (character.TilePosition.TileSqrDistanceTo(targetPosition)
                > DroneStartDistanceMax * DroneStartDistanceMax)
            {
                return false;
            }

            var targetTile = IsServer
                                 ? Server.World.GetTile(targetPosition)
                                 : Client.World.GetTile(targetPosition);

            if (targetTile.Height != character.Tile.Height)
            {
                return false;
            }

            // check for obstacles
            using var testResults = character.PhysicsBody.PhysicsSpace.TestLine(
                fromPosition: character.Position,
                toPosition: (targetPosition.X + 0.5,
                             targetPosition.Y + 0.5),
                collisionGroup: CollisionGroups.Default,
                sendDebugEvent: false);

            foreach (var testResult in testResults.AsList())
            {
                switch (testResult.PhysicsBody.AssociatedWorldObject?.ProtoGameObject)
                {
                    case IProtoObjectWall:
                    case IProtoObjectDoor:
                        // don't allow mining through walls and doors
                        hasObstacles = true;
                        return false;
                }
            }

            return true;
        }

        protected override void PrepareSystem()
        {
            base.PrepareSystem();
            if (IsClient)
            {
                ClientUpdateHelper.UpdateCallback += ClientUpdate;

                static void ClientUpdate()
                {
                    ClientSubmitStartDroneCommandsImmediately();
                }
            }
        }

        private void ClientRemote_OnDroneAbandoned(IProtoItemDrone protoItemDrone)
        {
            NotificationSystem.ClientShowNotification(
                                  Notification_DroneDeactivated_Title,
                                  Notification_DroneDeactivated_Message,
                                  NotificationColor.Bad,
                                  protoItemDrone.Icon)
                              .HideAfterDelay(delaySeconds: 120);
        }

        private void ClientRemote_OnDroneDestroyed(IProtoItemDrone protoItemDrone)
        {
            NotificationSystem.ClientShowNotification(
                                  Notification_DroneDestroyed_Title,
                                  message: null,
                                  NotificationColor.Bad,
                                  protoItemDrone.Icon)
                              .HideAfterDelay(delaySeconds: 120);
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered, timeInterval: 0.2)]
        private void ServerRemote_StartDrone(IItem itemDrone, Vector2Ushort worldPosition)
        {
            var character = ServerRemoteContext.Character;
            if (itemDrone.Container.Owner != character)
            {
                Logger.Info("Player don't own the drone: " + itemDrone, character);
                return;
            }

            if (itemDrone.ProtoItem is not IProtoItemDrone protoItemDrone)
            {
                // not a drone item
                Logger.Warning("Not a drone item: " + itemDrone, character);
                return;
            }

            var itemDroneControl = character.SharedGetPlayerSelectedHotbarItem();
            if (itemDroneControl.ProtoItem is not IProtoItemDroneControl)
            {
                Logger.Info("Don't have a drone remote control selected", character);
                return;
            }

            if (SharedIsMaxDronesToControlNumberExceeded(character))
            {
                Logger.Info("Exceeded max numbers of drones to control simultaneously", character);
                return;
            }

            if (SharedIsTargetAlreadyScheduledForAnyActiveDrone(character,
                                                                worldPosition,
                                                                logError: true))
            {
                return;
            }

            if (PlayerCharacter.GetPrivateState(character).IsDespawned
                || PlayerCharacter.GetPublicState(character).IsDead)
            {
                // despawned/dead player cannot start a drone
                return;
            }

            var protoDrone = protoItemDrone.ProtoDrone;
            if (!SharedIsValidStartLocation(character, worldPosition, out _))
            {
                Logger.Info("Too far to start a drone or has obstacles", character);
                return;
            }

            var objectDrone = itemDrone.GetPrivateState<ItemDronePrivateState>().WorldObjectDrone;

            if (!ServerIsMiningAllowed(worldPosition, objectDrone))
            {
                Logger.Info("Cannot mine there as it's already mined by another drone: " + worldPosition, character);
                return;
            }

            var targetObject = SharedGetCompatibleTarget(character,
                                                         worldPosition,
                                                         out _,
                                                         out _);
            if (targetObject is null)
            {
                Logger.Info("Nothing to mine there: " + worldPosition, character);
                return;
            }

            var durabilityValue = ItemDurabilitySystem.SharedGetDurabilityValue(itemDrone);
            if (durabilityValue == 0)
            {
                Logger.Warning("Cannot start a drone with 0 durability: " + itemDrone, character);
                return;
            }

            var dronePublicState = objectDrone.GetPublicState<DronePublicState>();
            var droneStructurePoints = durabilityValue
                                       * protoItemDrone.DurabilityToStructurePointsConversionCoefficient;
            dronePublicState.StructurePointsCurrent = (float)droneStructurePoints;

            var controlledDrones = character.SharedGetCurrentlyControlledDrones();
            controlledDrones.Add(objectDrone);
            Logger.Info("Drone control started: " + objectDrone);

            //Logger.Dev($"Drone start moving: {itemDrone} to {worldPosition}");
            protoDrone.ServerStartDrone(objectDrone,
                                        character);

            protoDrone.ServerSetDroneTarget(objectDrone,
                                            targetObject,
                                            fromStartPosition: character.Position);

            ServerItemUseObserver.NotifyItemUsed(character, itemDroneControl);
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered, timeInterval: 0.2)]
        private void ServerRemote_StartDrones(List<(IItem Item, Vector2Ushort Position)> drones)
        {
            foreach (var entry in drones)
            {
                this.ServerRemote_StartDrone(entry.Item, entry.Position);
            }
        }
    }
}