namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectPlayerLootContainer
        : ProtoStaticWorldObject
          <ObjectPlayerLootContainer.ObjectPlayerLootContainerPrivateState,
              ObjectPlayerLootContainer.ObjectPlayerLootContainerPublicState,
              StaticObjectClientState>,
          IInteractableProtoWorldObject,
          IProtoWorldObjectCustomInteractionCursor
    {
        public const double AutoDestroyTimeoutSeconds = 60 * 60; // 1 hour

        // {0} is another player name
        public const string MessageFormatLootFromAnotherPlayer = "Loot from {0}";

        public const string MessageLootFromCurrentPlayer = "Your loot";

        // {0} is another player name
        public const string NotificationCurrentPlayerItemsBeingLooted_Message =
            "{0} is going through your dropped items...";

        public const string NotificationCurrentPlayerItemsBeingLooted_Title = "Your items are being looted";

        // {0} is another player name
        public const string NotificationCurrentPlayerItemsTaken_Message =
            "{0} has taken all of your dropped items. Marker removed from the map.";

        public const string NotificationCurrentPlayerItemsTaken_Title = "Your items were taken!";

        private const double AutoDestroyPostponeSeconds = 60;

        private static readonly IItemsServerService ServerItemsService = IsServer ? Server.Items : null;

        public override double ClientUpdateIntervalSeconds => 1;

        public override string InteractionTooltipText => InteractionTooltipTexts.PickUp;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override StaticObjectKind Kind => StaticObjectKind.SpecialAllowDecals;

        public override string Name => "Player loot items";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SolidGround;

        public override double ObstacleBlockDamageCoef => 0; // not used

        public override double ServerUpdateIntervalSeconds => 0.5;

        public override float StructurePointsMax => float.MaxValue;

        public static IItemsContainer ServerTryCreateLootContainer(
            ICharacter character,
            Vector2D? position = null)
        {
            position ??= character.Position;

            return ServerTryCreateLootContainerInternal(character,
                                                        position.Value,
                                                        ensureNoWallsOnTheWay: true,
                                                        ensureNoClosedDoorsOnTheWay: true)
                   ?? ServerTryCreateLootContainerInternal(character,
                                                           position.Value,
                                                           ensureNoWallsOnTheWay: true,
                                                           ensureNoClosedDoorsOnTheWay: false)
                   ?? ServerTryCreateLootContainerInternal(character,
                                                           position.Value,
                                                           ensureNoWallsOnTheWay: false,
                                                           ensureNoClosedDoorsOnTheWay: false);
        }

        public BaseUserControlWithWindow ClientOpenUI(IWorldObject worldObject)
        {
            var itemsContainer = GetPrivateState((IStaticWorldObject)worldObject).ItemsContainer;
            var soundOpen = Client.UI.GetApplicationResource<SoundUI>("SoundWindowContainerBagOpen");
            var soundClose = Client.UI.GetApplicationResource<SoundUI>("SoundWindowContainerBagClose");
            return WindowContainerExchange.Show(itemsContainer,
                                                soundOpen,
                                                soundClose,
                                                isAutoClose: true);
        }

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            blueprint.SpriteRenderer.TextureResource = TextureResource.NoTexture;
        }

        public CursorId GetInteractionCursorId(bool isCanInteract)
        {
            return isCanInteract
                       ? CursorId.PickupPossible
                       : CursorId.PickupImpossible;
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            // don't call base implementation
            //base.ServerOnDestroy(gameObject);

            var lootObjectPrivateState = GetPrivateState(gameObject);
            var owner = lootObjectPrivateState.Owner;
            if (owner is null)
            {
                return;
            }

            // remove mark from map
            var characterPrivateState = PlayerCharacter.GetPrivateState(owner);
            var droppedLootLocations = characterPrivateState.DroppedLootLocations;
            for (var index = droppedLootLocations.Count - 1; index >= 0; index--)
            {
                var mark = droppedLootLocations[index];
                if (mark.Position == gameObject.TilePosition)
                {
                    droppedLootLocations.RemoveAt(index);
                }
            }

            var lastInteractingCharacter = lootObjectPrivateState.LastInteractingCharacter;
            if (owner == lastInteractingCharacter
                || lastInteractingCharacter is null)
            {
                // owner looted it or it's disappeared by timeout
                return;
            }

            // someone looted the owner items
            this.CallClient(owner,
                            _ => _.ClientRemote_NotifyLootFinished(lastInteractingCharacter.Name));
        }

        public void ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
            // nothing here
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            var ownerName = GetPublicState(worldObject).OwnerName;
            if (ownerName == character.Name)
            {
                return true;
            }

            if (PveSystem.SharedIsPve(false))
            {
                if (IsClient && PartySystem.ClientIsPartyMember(ownerName)
                    || (IsServer
                        && PartySystem.ServerIsSameParty(Server.Characters.GetPlayerCharacter(ownerName),
                                                         character)))
                {
                    // in PvE party members can pickup items of their party members
                }
                else
                {
                    // other players in PvE cannot pickup player's loot
                    if (writeToLog && IsClient)
                    {
                        PveSystem.ClientShowNotificationActionForbidden();
                    }

                    return false;
                }
            }

            if (NewbieProtectionSystem.SharedIsNewbie(character))
            {
                // newbie cannot pickup other players' loot
                if (writeToLog)
                {
                    NewbieProtectionSystem.SharedShowNewbieCannotDamageOtherPlayersOrLootBags(character,
                        isLootBag: true);
                }

                return false;
            }

            // non-newbie character can pickup players' loot
            // please note this validation has an override for derived ObjectPlayerLootContainerProtected
            return true;
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0.5, 0.15);
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            obstacleBlockDamageCoef = 0;
            damageApplied = 0;
            return false;
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
            Logger.Important($"{who} interacting with {worldObject}");
            var privateState = GetPrivateState((IStaticWorldObject)worldObject);
            privateState.LastInteractingCharacter = who;
            var owner = privateState.Owner;
            if (owner == who)
            {
                return;
            }

            this.CallClient(owner, _ => _.ClientRemote_NotifyLootInteraction(who.Name));
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            // don't use base implementation
            //base.ClientInitialize(data);

            var sceneObject = data.GameObject.ClientSceneObject;
            Client.Rendering.CreateSpriteRenderer(sceneObject,
                                                  this.DefaultTexture,
                                                  drawOrder: DrawOrder.Default,
                                                  positionOffset: (0.5, 0.5),
                                                  spritePivotPoint: (0.5, 0.5),
                                                  scale: 0.5);

            // attach a message about the loot owner
            var ownerName = data.PublicState.OwnerName;
            var message = ownerName == ClientCurrentCharacterHelper.Character?.Name
                              ? MessageLootFromCurrentPlayer
                              : string.Format(MessageFormatLootFromAnotherPlayer, ownerName);
            var positionOffset = this.SharedGetObjectCenterWorldOffset(data.GameObject)
                                 + (0, 0.8);
            var control = new WorldObjectTitleTooltip();
            control.Setup(message);

            Client.UI.AttachControl(sceneObject,
                                    control,
                                    positionOffset: positionOffset,
                                    isFocusable: false);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected override void ClientOnObjectDestroyed(Vector2D position)
        {
            // do nothing as currently it's not a damageable object
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource("StaticObjects/Loot/ObjectSack");
        }

        protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
        {
            tileRequirements
                .Clear()
                // skip this check as it's usually fine to drop the loot if player could stand there
                //.Add(ConstructionTileRequirements.ValidatorNoStaticObjectsExceptFloor)
                // validate no static physics objects there except destroyed walls and opened doors
                .Add(ConstructionTileRequirements.ValidatorSolidGroundOrPlatform)
                .Add(ConstructionTileRequirements.ValidatorNotCliffOrSlope)
                .Add(new ConstructionTileRequirements.Validator(
                         ConstructionTileRequirements.ErrorNoFreeSpace,
                         c => !ConstructionTileRequirements.TileHasAnyPhysicsObjectsWhere(
                                  c.Tile,
                                  t => t.PhysicsBody.IsStatic
                                       // allow destroyed walls physics in the tile
                                       && t.PhysicsBody.AssociatedWorldObject
                                           ?.ProtoWorldObject is not ObjectWallDestroyed
                                       // allow opened doors in the tile
                                       && !(t.PhysicsBody.AssociatedWorldObject
                                             ?.ProtoWorldObject is ProtoObjectDoor
                                            && t.PhysicsBody.AssociatedWorldObject
                                                .GetPublicState<ObjectDoorPublicState>().IsOpened))))
                // ensure no other loot containers
                .Add(new ConstructionTileRequirements.Validator(
                         // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                         "Tile already has a loot container",
                         c => !c.Tile.StaticObjects.Any(
                                  so => so.ProtoStaticWorldObject is ObjectPlayerLootContainer)));
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            // don't call the base method
            var privateState = data.PrivateState;
            var itemsContainer = privateState.ItemsContainer;

            if (itemsContainer is null)
            {
                // create container
                itemsContainer = ServerItemsService.CreateContainer<ItemsContainerOutputPublic>(
                    data.GameObject,
                    slotsCount: 1);
                privateState.ItemsContainer = itemsContainer;
            }
            else if (itemsContainer.ProtoItemsContainer is not ItemsContainerOutputPublic)
            {
                ServerItemsService.SetContainerType<ItemsContainerOutputPublic>(itemsContainer);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var worldObject = data.GameObject;
            var privateState = data.PrivateState;

            var itemsContainer = privateState.ItemsContainer;
            var hasItems = itemsContainer.OccupiedSlotsCount > 0;
            var isTimedOut = false;

            if (hasItems)
            {
                if (privateState.ItemsContainerLastHash
                    != privateState.ItemsContainer.StateHash)
                {
                    // items container updated - reset timeout
                    ServerSetDefaultDestroyTimeout(privateState);
                }
                else
                {
                    // there are items - check timeout
                    var timeNow = Api.Server.Game.FrameTime;
                    isTimedOut = timeNow >= privateState.DestroyAtTime;
                    if (isTimedOut)
                    {
                        // should destroy because timed out
                        if (Server.World.IsObservedByAnyPlayer(worldObject))
                        {
                            // cannot destroy - there are players observing it
                            isTimedOut = false;
                            privateState.DestroyAtTime = timeNow + AutoDestroyPostponeSeconds;
                        }
                    }
                }
            }

            if (hasItems
                && !isTimedOut)
            {
                return;
            }

            Logger.Important(
                "Destroying loot container at "
                + worldObject.TilePosition
                + (isTimedOut ? " - timed out" : " - contains no items"));

            Server.World.DestroyObject(worldObject);
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            // no damage
            obstacleBlockDamageCoef = 0;
            return 0;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((1, 1), group: CollisionGroups.ClickArea);
        }

        /// <summary>
        /// Finds the closest available empty tile (if possible).
        /// If cannot, returns the tile at the start position.
        /// </summary>
        private static Tile ServerGetStartTileForLootContainerLocation(Vector2D startPosition)
        {
            var startTile = Server.World.GetTile(startPosition.ToVector2Ushort());

            using var tempTiles = Api.Shared.GetTempList<Tile>();
            tempTiles.Add(startTile);
            tempTiles.AddRange(startTile.EightNeighborTiles);

            var collisionGroup = CollisionGroups.Default;
            var physicsSpace = Server.World.GetPhysicsSpace();
            tempTiles.AsList()
                     .SortBy(t => (t.Position.ToVector2D() + (0.5, 0.5))
                                 .DistanceSquaredTo(startPosition));

            foreach (var tile in tempTiles.AsList())
            {
                var testResults = physicsSpace.TestLine(startPosition,
                                                        toPosition: tile.Position.ToVector2D() + (0.5, 0.5),
                                                        collisionGroup,
                                                        sendDebugEvent: false);
                var isValidTile = true;
                foreach (var testResult in testResults.AsList())
                {
                    var associatedWorldObject = testResult.PhysicsBody.AssociatedWorldObject;
                    if ((associatedWorldObject is not null
                         && associatedWorldObject.IsStatic)
                        || testResult.PhysicsBody.AssociatedProtoTile is not null)
                    {
                        isValidTile = false;
                        break;
                    }
                }

                if (isValidTile)
                {
                    return tile;
                }
            }

            return startTile;
        }

        private static void ServerSetDefaultDestroyTimeout(ObjectPlayerLootContainerPrivateState privateState)
        {
            var timeNow = Server.Game.FrameTime;
            privateState.DestroyAtTime = timeNow + AutoDestroyTimeoutSeconds;
            privateState.ItemsContainerLastHash = privateState.ItemsContainer.StateHash;
        }

        private static IItemsContainer ServerTryCreateLootContainerAtPosition(
            Vector2Ushort tilePosition,
            ICharacter character,
            bool writeWarningsToLog = true)
        {
            IStaticWorldObject objectLootContainer = null;
            var protoLootContainer = NewbieProtectionSystem.SharedIsNewbie(character)
                                         ? Api.GetProtoEntity<ObjectPlayerLootContainerProtected>()
                                         : Api.GetProtoEntity<ObjectPlayerLootContainer>();

            if (protoLootContainer.CheckTileRequirements(tilePosition, character: null, logErrors: false))
            {
                Logger.Important("Creating loot container at " + tilePosition);
                objectLootContainer = Server.World.CreateStaticWorldObject(protoLootContainer, tilePosition);
            }

            if (objectLootContainer is null)
            {
                // cannot create loot container
                if (writeWarningsToLog)
                {
                    Logger.Warning(
                        $"Cannot create loot container at {tilePosition} - tile contains something preventing it.");
                }

                return null;
            }

            var lootPrivateState = GetPrivateState(objectLootContainer);
            lootPrivateState.Owner = character;
            ServerSetDefaultDestroyTimeout(lootPrivateState);
            GetPublicState(objectLootContainer).OwnerName = character.Name;

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            characterPrivateState.DroppedLootLocations.Add(
                new DroppedLootInfo(tilePosition, lootPrivateState.DestroyAtTime));

            return lootPrivateState.ItemsContainer;
        }

        // try create the loot container nearby the character death position
        private static IItemsContainer ServerTryCreateLootContainerInternal(
            ICharacter character,
            Vector2D startPosition,
            bool ensureNoWallsOnTheWay,
            bool ensureNoClosedDoorsOnTheWay)
        {
            var startTile = ServerGetStartTileForLootContainerLocation(startPosition);

            var checkQueue = new List<Tile>();
            checkQueue.Add(startTile);

            var checkedTiles = new HashSet<Vector2Ushort>();
            checkedTiles.Add(startTile.Position);

            // try to drop the loot at max 15 tiles away from the requested tile
            const byte maxNeighborFindIterations = 15;
            var iterationAttempt = 0;

            while (true)
            {
                iterationAttempt++;

                foreach (var tile in checkQueue)
                {
                    var lootContainer = ServerTryCreateLootContainerAtPosition(
                        tile.Position,
                        character,
                        writeWarningsToLog: false);
                    if (lootContainer is not null)
                    {
                        // successfully created a loot container
                        return lootContainer;
                    }
                }

                if (iterationAttempt >= maxNeighborFindIterations)
                {
                    // unable to create a loot container
                    return null;
                }

                // no container spawned - prepare for the next attempt
                // enqueue all the neighbor tiles for each of the tiles checked in the current iteration
                var checkedQueueLength = checkQueue.Count;
                for (var index = 0; index < checkedQueueLength; index++)
                {
                    var tile = checkQueue[index];
                    if (tile.IsValidTile)
                    {
                        EnqueueNeighbors(tile);
                    }
                }

                // trim checked queue items
                checkQueue.RemoveRange(0, checkedQueueLength);

                if (checkQueue.Count == 0)
                {
                    // unable to create a loot container and neighbor tiles are not candidates for loot spawn
                    return null;
                }
            }

            // helper method to enqueue not yet checked tiles from the tile neighbors
            void EnqueueNeighbors(Tile tile)
            {
                for (byte direction = 0; direction < 8; direction++)
                {
                    var neighborTile = GetNeighborTile(in tile, direction);
                    if (!checkedTiles.Add(neighborTile.Position))
                    {
                        // the tile is already checked
                        continue;
                    }

                    var isValidTile = true;

                    if (!neighborTile.IsValidTile)
                    {
                        isValidTile = false;
                    }
                    else if (ensureNoWallsOnTheWay
                             || ensureNoClosedDoorsOnTheWay)
                    {
                        if (neighborTile.IsCliff)
                        {
                            isValidTile = false;
                        }
                        else
                        {
                            foreach (var staticObject in neighborTile.StaticObjects)
                            {
                                if (ensureNoWallsOnTheWay
                                    && staticObject.ProtoStaticWorldObject is IProtoObjectWall)
                                {
                                    isValidTile = false;
                                    break;
                                }

                                if (ensureNoClosedDoorsOnTheWay
                                    && staticObject.ProtoStaticWorldObject is IProtoObjectDoor
                                    && !staticObject.GetPublicState<ObjectDoorPublicState>().IsOpened)
                                {
                                    isValidTile = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (isValidTile)
                    {
                        checkQueue.Add(neighborTile);
                    }
                }
            }

            // helper method to select neighbor tile
            static Tile GetNeighborTile(in Tile tile, byte direction)
            {
                return direction switch
                {
                    0 => tile.NeighborTileLeft,
                    1 => tile.NeighborTileUp,
                    2 => tile.NeighborTileRight,
                    3 => tile.NeighborTileDown,
                    // diagonal directions are farther away so they checked last
                    4 => tile.NeighborTileUpLeft,
                    5 => tile.NeighborTileUpRight,
                    6 => tile.NeighborTileDownRight,
                    7 => tile.NeighborTileDownLeft,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        private void ClientRemote_NotifyLootFinished(string name)
        {
            NotificationSystem.ClientShowNotification(
                                  NotificationCurrentPlayerItemsTaken_Title,
                                  string.Format(NotificationCurrentPlayerItemsTaken_Message, name),
                                  NotificationColor.Bad,
                                  this.DefaultTexture)
                              .HideAfterDelay(delaySeconds: 10 * 60);
        }

        private void ClientRemote_NotifyLootInteraction(string name)
        {
            NotificationSystem.ClientShowNotification(
                                  NotificationCurrentPlayerItemsBeingLooted_Title,
                                  string.Format(NotificationCurrentPlayerItemsBeingLooted_Message, name),
                                  NotificationColor.Bad,
                                  this.DefaultTexture)
                              .HideAfterDelay(delaySeconds: 10 * 60);
        }

        public class ObjectPlayerLootContainerPrivateState : StructurePrivateState
        {
            public double DestroyAtTime { get; set; }

            [SyncToClient]
            public IItemsContainer ItemsContainer { get; set; }

            public int? ItemsContainerLastHash { get; set; }

            public ICharacter LastInteractingCharacter { get; set; }

            public ICharacter Owner { get; set; }
        }

        public class ObjectPlayerLootContainerPublicState : StaticObjectPublicState
        {
            [SyncToClient]
            public string OwnerName { get; set; }
        }
    }
}