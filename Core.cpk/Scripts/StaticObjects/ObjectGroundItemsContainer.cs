namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Windows;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public sealed class ObjectGroundItemsContainer
        : ProtoWorldObject<IStaticWorldObject,
              ObjectGroundItemsContainer.PrivateState,
              ObjectGroundItemsContainer.PublicState,
              EmptyClientState>,
          IProtoWorldObjectCustomInteractionCursor,
          IInteractableProtoWorldObject
    {
        public const string NotificationCannotDropItemThere = "Cannot drop item there.";

        public const string NotificationNoFreeSpaceToDrop = "No free place to drop item";

        private const double AutoDestroyPostponeSeconds = 60;

        // Remove ground items container after this timeout (if nobody is observing it).
        private const double AutoDestroyTimeoutSeconds = 5 * 60;

        private const double ClickAreaRadius = 0.45;

        private const byte DefaultMaxSlotsCount = 4;

        public static readonly TextureResource TextureResourceSack
            = new TextureResource("StaticObjects/Loot/ObjectSack");

        private static readonly IItemsServerService ServerItems = IsServer ? Server.Items : null;

        private static ObjectGroundItemsContainer instance;

        private readonly IConstructionTileRequirementsReadOnly tileRequirements
            = new ConstructionTileRequirements()
              .Add(ConstructionTileRequirements.ValidatorNoStaticObjectsExceptFloor)
              .Add(ConstructionTileRequirements.ValidatorSolidGround)
              .Add(ConstructionTileRequirements.ValidatorNotCliffOrSlope);

        public ObjectGroundItemsContainer()
        {
            instance = this;
        }

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public ITextureResource DefaultTexture => TextureResource.NoTexture;

        public ITextureResource Icon => null;

        public override string InteractionTooltipText => InteractionTooltipTexts.PickUp;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public bool IsIgnoredBySpawnScripts => false;

        // allow terrain decals under it
        public StaticObjectKind Kind => StaticObjectKind.SpecialAllowDecals;

        public StaticObjectLayoutReadOnly Layout { get; } = StaticObjectLayout.DefaultOneTileLayout;

        public override string Name => "Ground items";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SolidGround;

        public override double ServerUpdateIntervalSeconds => 0.5;

        public double StructureExplosiveDefenseCoef => 0;

        public float StructurePointsMax => 100;

        public BoundsInt ViewBounds { get; } = StaticObjectLayout.DefaultOneTileLayout.Bounds;

        public static async void ClientTryDropItemOnGround(
            IItem itemToDrop,
            ushort countToDrop,
            Vector2Ushort? dropTilePosition = null)
        {
            countToDrop = Math.Min(countToDrop, itemToDrop.Count);

            var character = Client.Characters.CurrentPlayerCharacter;
            if (!dropTilePosition.HasValue)
            {
                if (ClientTryDropItemToGroundContainerNearby(
                    character.Tile,
                    itemToDrop,
                    countToDrop,
                    out dropTilePosition,
                    out var resultItemsContainer))
                {
                    OnSuccess(resultItemsContainer);
                    return;
                }

                countToDrop = Math.Min(countToDrop, itemToDrop.Count);

                var obstaclesOnTheWay = false;
                if (!dropTilePosition.HasValue
                    || !SharedIsWithinInteractionDistance(
                        character,
                        dropTilePosition.Value,
                        out obstaclesOnTheWay))
                {
                    NotificationSystem.ClientShowNotification(
                        obstaclesOnTheWay
                            ? CoreStrings.Notification_ObstaclesOnTheWay
                            : NotificationNoFreeSpaceToDrop,
                        color: NotificationColor.Bad,
                        icon: TextureResourceSack);
                    return;
                }
            }

            var tilePosition = dropTilePosition.Value;
            if (!SharedIsWithinInteractionDistance(
                    character,
                    tilePosition,
                    out var obstaclesOnTheWay2))
            {
                NotificationSystem.ClientShowNotification(
                    obstaclesOnTheWay2
                        ? CoreStrings.Notification_ObstaclesOnTheWay
                        : CoreStrings.Notification_TooFar,
                    NotificationCannotDropItemThere,
                    NotificationColor.Bad,
                    TextureResourceSack);
                return;
            }

            var tile = Client.World.GetTile(tilePosition);
            var objectGroundContainer = tile.StaticObjects.FirstOrDefault(_ => _.ProtoGameObject == instance);
            if (objectGroundContainer is null)
            {
                if (!instance.CheckTileRequirements(tilePosition, character, logErrors: false))
                {
                    // cannot drop item here
                    NotificationSystem.ClientShowNotification(
                        CoreStrings.Notification_ObstaclesOnTheWay,
                        NotificationCannotDropItemThere,
                        NotificationColor.Bad,
                        TextureResourceSack);
                    return;
                }

                Logger.Info(
                    $"Requested placing item on the ground (new ground container needed): {itemToDrop}. Count={countToDrop}.");
                objectGroundContainer = await instance.CallServer(
                                            _ => _.ServerRemote_DropItemOnGround(
                                                itemToDrop,
                                                countToDrop,
                                                tilePosition));
                if (objectGroundContainer is not null)
                {
                    // successfully placed on ground
                    OnSuccess(GetPublicState(objectGroundContainer).ItemsContainer);
                    return;
                }

                // we're continue the async call - the context might have been changed
                if (itemToDrop.IsDestroyed)
                {
                    return;
                }

                // was unable to place the item on the ground - maybe it was already placed with an earlier call
                if (itemToDrop.Container?.OwnerAsStaticObject?.ProtoStaticWorldObject is ObjectGroundItemsContainer)
                {
                    // it seems to be on the ground now
                    return;
                }

                // the action is definitely failed
                instance.SoundPresetObject.PlaySound(ObjectSound.InteractFail);
                return;
            }

            if (!instance.SharedCanInteract(character, objectGroundContainer, writeToLog: true))
            {
                return;
            }

            // get items container instance
            var groundItemsContainer = GetPublicState(objectGroundContainer).ItemsContainer;

            // try move item to the ground items container
            if (!Client.Items.MoveOrSwapItem(
                    itemToDrop,
                    groundItemsContainer,
                    countToMove: countToDrop,
                    isLogErrors: false))
            {
                // cannot move - open container UI
                ClientOpenContainerExchangeUI(objectGroundContainer);
                return;
            }

            // item moved successfully
            OnSuccess(groundItemsContainer);

            void OnSuccess(IItemsContainer resultGroundItemsContainer)
            {
                itemToDrop.ProtoItem.ClientOnItemDrop(itemToDrop, resultGroundItemsContainer);

                NotificationSystem.ClientShowItemsNotification(
                    itemsChangedCount: new Dictionary<IProtoItem, int>()
                        { { itemToDrop.ProtoItem, -countToDrop } });

                if (Api.Client.Input.IsKeyHeld(InputKey.Shift, evenIfHandled: true))
                {
                    // open container UI to allow faster items exchange with it
                    ClientOpenContainerExchangeUI(resultGroundItemsContainer.OwnerAsStaticObject);
                }
            }
        }

        public static bool ClientTryDropItemToGroundContainerNearby(
            Tile startTile,
            IItem item,
            ushort countToDrop,
            out Vector2Ushort? fallbackDropPosition,
            out IItemsContainer resultItemsContainer)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            var objectGroundContainer = startTile.StaticObjects.FirstOrDefault(
                so => so.ProtoGameObject is ObjectGroundItemsContainer);

            if (objectGroundContainer is not null)
            {
                if (TryDropTo(objectGroundContainer, out var droppedTo))
                {
                    // dropped successfully
                    fallbackDropPosition = null;
                    resultItemsContainer = droppedTo;
                    return true;
                }
            }
            else if (instance.CheckTileRequirements(startTile.Position, character, logErrors: false))
            {
                // can create a new ground items container there
                fallbackDropPosition = startTile.Position;
                resultItemsContainer = null;
                return false; // returning false - will try to create a new container there
            }

            // collect neighbor tiles list which are accessible by the player
            var neighborTiles = startTile.EightNeighborTiles
                                         .SelectMany(t => t.EightNeighborTiles)
                                         .Where(t => SharedIsWithinInteractionDistance(character, t.Position, out _))
                                         .OrderBy(t => t.Position.TileSqrDistanceTo(startTile.Position))
                                         .Distinct()
                                         .ToList();

            // try drop to an existing ground container nearby
            foreach (var neighborTile in neighborTiles)
            {
                if (neighborTile.Height != startTile.Height)
                {
                    // different tile height
                    continue;
                }

                objectGroundContainer = neighborTile.StaticObjects.FirstOrDefault(
                    so => so.ProtoGameObject is ObjectGroundItemsContainer);
                if (objectGroundContainer is null)
                {
                    continue;
                }

                if (TryDropTo(objectGroundContainer, out var droppedTo))
                {
                    // dropped successfully
                    fallbackDropPosition = null;
                    resultItemsContainer = droppedTo;
                    return true;
                }
            }

            // cannot find any ground container nearby
            // let's find any tile nearby suitable for a new ground container
            neighborTiles.Shuffle(); // randomize neighbors

            foreach (var neighborTile in neighborTiles)
            {
                if (neighborTile.Height != startTile.Height)
                {
                    // different tile height
                    continue;
                }

                if (instance.CheckTileRequirements(neighborTile.Position, character, logErrors: false))
                {
                    // this neighbor tile is suitable for a new ground container
                    fallbackDropPosition = neighborTile.Position;
                    resultItemsContainer = null;
                    return false; // returning false - will try to create a new container there
                }
            }

            fallbackDropPosition = null;
            resultItemsContainer = null;
            return false;

            bool TryDropTo(IStaticWorldObject staticWorldObject, out IItemsContainer droppedTo)
            {
                droppedTo = GetPublicState(staticWorldObject).ItemsContainer;
                return Client.Items.MoveOrSwapItem(item,
                                                   droppedTo,
                                                   allowSwapping: false,
                                                   countToMove: countToDrop,
                                                   isLogErrors: false);
            }
        }

        public static void ServerSetDestructionTimeout(IStaticWorldObject worldObject, double destroyTimeoutSeconds)
        {
            var publicState = GetPublicState(worldObject);
            var timeNow = Server.Game.FrameTime;
            publicState.DestroyAtTime = timeNow + destroyTimeoutSeconds;
        }

        public static void ServerTrimSlotsNumber(IItemsContainer itemsContainer)
        {
            if (itemsContainer.IsDestroyed)
            {
                return;
            }

            Server.Items.SetSlotsCount(itemsContainer,
                                       Math.Max(DefaultMaxSlotsCount,
                                                itemsContainer.OccupiedSlotsCount));

            if (itemsContainer.SlotsCount > DefaultMaxSlotsCount)
            {
                // change ground container type (prevent from placing new items there)
                ServerItems.SetContainerType<ItemsContainerOutputPublic>(itemsContainer);
            }
        }

        public static IItemsContainer ServerTryDropOnGroundContainerContent(
            Tile tile, 
            IItemsContainer otherContainer,
            double? destroyTimeout = null)
        {
            var otherContainerOccupiedSlotsCount = otherContainer?.OccupiedSlotsCount ?? 0;
            if (otherContainerOccupiedSlotsCount == 0)
            {
                // nothing to drop there
                return null;
            }

            var groundContainer = ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(forCharacter: null, tile);
            if (groundContainer is null)
            {
                // cannot drop items there
                return null;
            }

            ServerItems.TryMoveAllItems(
                containerFrom: otherContainer,
                containerTo: groundContainer);

            SharedLootDropNotifyHelper.ServerOnLootDropped(groundContainer);

            if (destroyTimeout.HasValue)
            {
                ServerSetDestructionTimeout(
                    (IStaticWorldObject)groundContainer.Owner,
                    destroyTimeout.Value);
            }

            return groundContainer;
        }

        /// <summary>
        /// Returns a ground container for the provided tile.<br />
        /// Please note: the ground container will be automatically destroyed if (during the next update) it's empty!
        /// </summary>
        public static IItemsContainer ServerTryGetOrCreateGroundContainerAtTile(
            ICharacter forCharacter,
            Vector2Ushort tilePosition,
            bool writeWarningsToLog = true)
        {
            var tile = Server.World.GetTile(tilePosition);
            var objectGroundContainer = tile.StaticObjects.FirstOrDefault(
                so => so.ProtoGameObject is ObjectGroundItemsContainer);

            if (objectGroundContainer is null)
            {
                // No ground container found in the cell. Try to create a new ground container here.
                objectGroundContainer = TryCreateGroundContainer();
                if (objectGroundContainer is null)
                {
                    // cannot create
                    return null;
                }
            }
            else if (!WorldObjectClaimSystem.SharedIsAllowInteraction(forCharacter,
                                                                      objectGroundContainer,
                                                                      showClientNotification: false))
            {
                // cannot interact with this ground container as it's claimed by another player
                return null;
            }

            var result = GetPublicState(objectGroundContainer).ItemsContainer;
            ServerExpandContainerAndScheduleProcessing(result);
            return result;

            IStaticWorldObject TryCreateGroundContainer()
            {
                if (instance.CheckTileRequirements(tilePosition, character: null, logErrors: false))
                {
                    Logger.Info("Creating ground container at " + tilePosition);
                    return Server.World.CreateStaticWorldObject(instance, tilePosition);
                }

                if (writeWarningsToLog)
                {
                    Logger.Warning(
                        $"Cannot create ground container at {tilePosition} - tile contains something preventing it.");
                }

                return null;
            }
        }

        public static IItemsContainer ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(
            [CanBeNull] ICharacter forCharacter,
            Tile tile,
            bool canExceedContainerSpace = true)
        {
            var groundContainer = ServerTryGetOrCreateGroundContainerAtTile(
                forCharacter,
                tile.Position,
                writeWarningsToLog: false);

            if (groundContainer is not null
                && (groundContainer.OccupiedSlotsCount
                    < (canExceedContainerSpace
                           ? byte.MaxValue / 2
                           : groundContainer.SlotsCount)))
            {
                // found a ground container with empty space
                ServerExpandContainerAndScheduleProcessing(groundContainer);
                return groundContainer;
            }

            // try to get or create a container at the neighbor tiles
            var neighborTiles = tile.EightNeighborTiles.ToList();
            neighborTiles.Shuffle(); // randomize neighbors

            // try obtain the ground container nearby
            foreach (var neighborTile in neighborTiles)
            {
                if (neighborTile.Height != tile.Height)
                {
                    // different tile height
                    continue;
                }

                groundContainer = ServerTryGetOrCreateGroundContainerAtTile(
                    forCharacter,
                    neighborTile.Position,
                    writeWarningsToLog: false);
                if (groundContainer is not null
                    && (groundContainer.OccupiedSlotsCount
                        < (canExceedContainerSpace
                               ? byte.MaxValue / 2
                               : groundContainer.SlotsCount)))
                {
                    // found a ground container with empty space
                    ServerExpandContainerAndScheduleProcessing(groundContainer);
                    return groundContainer;
                }
            }

            return null;
        }

        public bool CheckTileRequirements(Vector2Ushort startTilePosition, ICharacter character, bool logErrors)
        {
            return this.tileRequirements.Check(this, startTilePosition, null, logErrors);
        }

        public override string ClientGetTitle(IWorldObject worldObject)
        {
            return null;
        }

        public BaseUserControlWithWindow ClientOpenUI(IWorldObject worldObject)
        {
            var itemsContainer = GetPublicState((IStaticWorldObject)worldObject).ItemsContainer;
            var soundOpen = Client.UI.GetApplicationResource<SoundUI>("SoundWindowContainerBagOpen");
            var soundClose = Client.UI.GetApplicationResource<SoundUI>("SoundWindowContainerBagClose");
            return WindowContainerExchange.Show(itemsContainer,
                                                soundOpen,
                                                soundClose,
                                                isAutoClose: true);
        }

        public void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            blueprint.SpriteRenderer.TextureResource = TextureResource.NoTexture;
        }

        public CursorId GetInteractionCursorId(bool isCanInteract)
        {
            return isCanInteract
                       ? CursorId.PickupPossible
                       : CursorId.PickupImpossible;
        }

        public StaticObjectLayoutReadOnly GetLayout(IStaticWorldObject worldObject)
        {
            return this.Layout;
        }

        public IStaticWorldObject ServerRemote_DropItemOnGround(
            IItem item,
            ushort countToDrop,
            Vector2Ushort tilePosition)
        {
            var character = ServerRemoteContext.Character;
            if (item is null)
            {
                Logger.Error(
                    "Cannot drop item - the item is not found",
                    character);
                return null;
            }

            if (countToDrop > item.Count)
            {
                Logger.Error(
                    $"Cannot drop item: {item} - count to drop={countToDrop} is > than available item count. Will request a container re-sync.",
                    character);
                ServerItems.ServerForceContainersResync(character);
                return null;
            }

            if (item.Container.OwnerAsStaticObject?.ProtoStaticWorldObject is ObjectGroundItemsContainer)
            {
                Logger.Warning(
                    $"Cannot drop item: {item} - already dropped to ground",
                    character);
                return null;
            }

            item.Container.ProtoItemsContainer.SharedValidateCanInteract(
                character,
                item.Container,
                writeToLog: true);

            if (!SharedIsWithinInteractionDistance(
                    character,
                    tilePosition,
                    out _))
            {
                Logger.Error(
                    $"Cannot drop item: {item} - character is too far from the requested tile position or there are obstacles on the way.",
                    character);
                return null;
            }

            var groundItemsContainer = ServerTryGetOrCreateGroundContainerAtTile(character, tilePosition);
            if (groundItemsContainer is null)
            {
                return null;
            }

            // try move item to the ground items container
            if (!ServerItems.MoveOrSwapItem(
                    item,
                    groundItemsContainer,
                    out _,
                    countToMove: countToDrop,
                    byCharacter: character,
                    sendUpdateToCharacter: true))
            {
                if (groundItemsContainer.OccupiedSlotsCount == 0)
                {
                    Server.World.DestroyObject(groundItemsContainer.OwnerAsStaticObject);
                }

                return null;
            }

            // notify other players
            using (var scopedBy = Api.Shared.GetTempList<ICharacter>())
            {
                Server.World.GetScopedByPlayers(groundItemsContainer.Owner, scopedBy);
                scopedBy.Remove(character);
                this.CallClient(scopedBy.AsList(), _ => _.ClientRemote_OtherPlayerDroppedItem(tilePosition));
            }

            return groundItemsContainer.OwnerAsStaticObject;
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            // don't use the base implementation as it will not work in PvE
            // (action forbidden if player doesn't have access to the land claim)
            if (character.GetPublicState<ICharacterPublicState>().IsDead
                || IsServer && !character.ServerIsOnline)
            {
                return false;
            }

            if (!NewbieProtectionSystem.SharedValidateInteractionIsNotForbidden(character, worldObject, writeToLog)
                || !WorldObjectClaimSystem.SharedIsAllowInteraction(character, worldObject, writeToLog))
            {
                return false;
            }

            return this.SharedIsInsideCharacterInteractionArea(character, worldObject, writeToLog);
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0.5, 0.15);
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
            // actually, we don't need client-server interaction for this container 
            // as client is aware about the items container
            var privateState = GetPrivateState((IStaticWorldObject)worldObject);
            privateState.ServerLastInteractCharacter = who;
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
            // actually, we don't need client-server interaction for this container 
            // as client is aware about the items container
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            // don't use base implementation
            //base.ClientInitialize(data);

            var sceneObject = data.GameObject.ClientSceneObject;
            sceneObject.AddComponent<ComponentObjectGroundItemsContainerRenderer>()
                       .Setup(data.PublicState.ItemsContainer);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            if (ClientItemsManager.ItemInHand is not null)
            {
                // in that case we want to allow place item from the hand
                // it will be handled automatically via ClientTryDropItemOnGround() call
                return;
            }

            var currentPlayerCharacter = Client.Characters.CurrentPlayerCharacter;
            var containerGround = data.PublicState.ItemsContainer;

            // can pickup all objects if these are simply items dropped on the ground and not an item sack
            // which is automatically displayed when there are more than 4 items
            if (containerGround.OccupiedSlotsCount <= 4
                && !Api.Client.Input.IsKeyHeld(InputKey.Control, evenIfHandled: true)
                && !Api.Client.Input.IsKeyHeld(InputKey.Alt,     evenIfHandled: true))
            {
                // try pickup all the items
                var result = currentPlayerCharacter.ProtoCharacter.ClientTryTakeAllItems(
                    currentPlayerCharacter,
                    containerGround,
                    showNotificationIfInventoryFull: false);
                if (result.MovedItems.Count > 0)
                {
                    // at least one item taken from ground
                    NotificationSystem.ClientShowItemsNotification(
                        itemsChangedCount: result.MovedItems
                                                 .GroupBy(p => p.Key.ProtoItem)
                                                 .ToDictionary(p => p.Key, p => p.Sum(v => v.Value)));
                }
            }

            if (containerGround.OccupiedSlotsCount > 0)
            {
                ClientOpenContainerExchangeUI(data.GameObject);
            }
        }

        protected override void ClientOnObjectDestroyed(Vector2D position)
        {
            // do nothing as currently it's not a damageable object
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            var publicState = data.PublicState;
            if (publicState.ItemsContainer is null)
            {
                // create container
                publicState.ItemsContainer
                    = ServerItems.CreateContainer<ItemsContainerPublic>(
                        data.GameObject,
                        slotsCount: DefaultMaxSlotsCount);
            }
            else if (publicState.ItemsContainer.SlotsCount < DefaultMaxSlotsCount)
            {
                ServerItems.SetSlotsCount(publicState.ItemsContainer, slotsCount: DefaultMaxSlotsCount);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var worldObject = data.GameObject;
            var publicState = data.PublicState;

            var itemsContainer = publicState.ItemsContainer;
            var hasItems = itemsContainer.OccupiedSlotsCount > 0;
            var isTimedOut = false;

            if (hasItems)
            {
                var timeNow = Server.Game.FrameTime;
                if (publicState.ItemsContainerLastHash
                    != publicState.ItemsContainer.StateHash)
                {
                    // items container updated - try to extend the timeout
                    publicState.DestroyAtTime = Math.Max(publicState.DestroyAtTime,
                                                         timeNow + AutoDestroyTimeoutSeconds);
                    publicState.ItemsContainerLastHash = publicState.ItemsContainer.StateHash;
                }
                else
                {
                    // there are items - check timeout
                    isTimedOut = timeNow >= publicState.DestroyAtTime;
                    if (isTimedOut)
                    {
                        // should destroy because timed out
                        if (Server.World.IsObservedByAnyPlayer(worldObject))
                        {
                            // cannot destroy - there are players observing it
                            isTimedOut = false;
                            publicState.DestroyAtTime = timeNow + AutoDestroyPostponeSeconds;
                        }
                    }
                }

                if (!isTimedOut)
                {
                    return;
                }
            }

            // don't have items or timeout reached
            Logger.Info(
                "Destroying ground container at "
                + worldObject.TilePosition
                + (isTimedOut ? " - timed out" : " - contains no items"));

            if (data.PrivateState.ServerLastInteractCharacter is not null)
            {
                // notify other players that the ground items were picked up
                using var scopedBy = Api.Shared.GetTempList<ICharacter>();
                Server.World.GetScopedByPlayers(worldObject, scopedBy);
                scopedBy.Remove(data.PrivateState.ServerLastInteractCharacter);
                this.CallClient(scopedBy.AsList(), _ => _.ClientRemote_OtherPlayerPickedUp(worldObject.TilePosition));
            }

            // actually destroy it
            publicState.ItemsContainer = null;
            Server.World.DestroyObject(worldObject);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: ClickAreaRadius,
                    center: (0.5, 0.5),
                    @group: CollisionGroups.ClickArea);
        }

        private static void ClientOpenContainerExchangeUI(IStaticWorldObject objectGroundContainer)
        {
            // actually, we don't need client-server interaction for this container 
            // as client is aware about the items container
            InteractableWorldObjectHelper.ClientStartInteract(objectGroundContainer);
        }

        /// <summary>
        /// This method will expand the container capacity to max and schedule its trimming.
        /// </summary>
        private static void ServerExpandContainerAndScheduleProcessing(IItemsContainer itemsContainer)
        {
            ServerItems.SetSlotsCount(itemsContainer, slotsCount: byte.MaxValue);
            ServerTimersSystem.AddAction(0, () => ServerTrimSlotsNumber(itemsContainer));
        }

        private static bool SharedIsWithinInteractionDistance(
            ICharacter character,
            Vector2Ushort tilePosition,
            out bool obstaclesOnTheWay)
        {
            var interactionAreaShape = character.PhysicsBody.Shapes.FirstOrDefault(
                s => s.CollisionGroup == CollisionGroups.CharacterInteractionArea);

            if (interactionAreaShape is null)
            {
                // no interaction area shape (probably a spectator character)
                obstaclesOnTheWay = false;
                return false;
            }

            var penetration = character.PhysicsBody.PhysicsSpace.TestShapeCollidesWithShape(
                sourceShape: interactionAreaShape,
                targetShape: new CircleShape(
                    center: tilePosition.ToVector2D() + (0.5, 0.5),
                    radius: ClickAreaRadius,
                    collisionGroup: CollisionGroups.ClickArea),
                sourceShapeOffset: character.PhysicsBody.Position);

            if (!penetration.HasValue)
            {
                // outside of interaction area
                obstaclesOnTheWay = false;
                return false;
            }

            // check that there are no other objects on the way between them (defined by default layer)
            var physicsSpace = character.PhysicsBody.PhysicsSpace;
            var characterCenter = character.Position + character.PhysicsBody.CenterOffset;
            var worldObjectCenter = (Vector2D)tilePosition + new Vector2D(0.5, 0.5);
            var worldObjectPointClosestToCharacter = new BoundsInt(tilePosition, Vector2Int.One)
                .ClampInside(characterCenter);

            obstaclesOnTheWay = ObstacleTestHelper.SharedHasObstaclesOnTheWay(characterCenter,
                                                                              physicsSpace,
                                                                              worldObjectCenter,
                                                                              worldObjectPointClosestToCharacter,
                                                                              sendDebugEvents: false);

            return !obstaclesOnTheWay;
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_OtherPlayerDroppedItem(Vector2Ushort position)
        {
            Client.Audio.PlayOneShot(ItemsSoundPresets.SoundResourceOtherPlayerDropItem,
                                     position.ToVector2D() + this.Layout.Center);
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_OtherPlayerPickedUp(Vector2Ushort position)
        {
            Client.Audio.PlayOneShot(ItemsSoundPresets.SoundResourceOtherPlayerPickItem,
                                     position.ToVector2D() + this.Layout.Center);
        }

        public class PrivateState : BasePrivateState
        {
            [TempOnly]
            public ICharacter ServerLastInteractCharacter { get; set; } // this property is not synchronized
        }

        public class PublicState : BasePublicState, IWorldObjectPublicStateWithClaim
        {
            [SyncToClient]
            public double DestroyAtTime { get; set; }

            [SyncToClient]
            public IItemsContainer ItemsContainer { get; set; }

            public int? ItemsContainerLastHash { get; set; }

            [SyncToClient]
            [TempOnly]
            public ILogicObject WorldObjectClaim { get; set; }
        }
    }
}