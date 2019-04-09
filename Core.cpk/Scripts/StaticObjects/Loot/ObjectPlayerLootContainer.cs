namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Windows;
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

    public sealed class ObjectPlayerLootContainer
        : ProtoStaticWorldObject
          <ObjectPlayerLootContainer.ObjectPlayerLootContainerPrivateState,
              ObjectPlayerLootContainer.ObjectPlayerLootContainerPublicState,
              StaticObjectClientState>,
          IInteractableProtoStaticWorldObject,
          IProtoStaticWorldObjectCustomInteractionCursor
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

        private static ObjectPlayerLootContainer instance;

        public ObjectPlayerLootContainer()
        {
            instance = this;
        }

        public override double ClientUpdateIntervalSeconds => 1;

        public override string InteractionTooltipText => InteractionTooltipTexts.PickUp;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        // this is a "natural object" to allow decals to show under it
        public override StaticObjectKind Kind => StaticObjectKind.NaturalObject;

        public override string Name => "Player loot items";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.SolidGround;

        public override double ObstacleBlockDamageCoef => 0; // not used

        public override double ServerUpdateIntervalSeconds => 1;

        public override float StructurePointsMax => float.MaxValue;

        public static IItemsContainer ServerTryCreateLootContainer(ICharacter character)
        {
            var tile = character.Tile;
            return ServerTryCreateLootContainerInternal(character,    tile, ensureTheHeightIsTheSame: true)
                   ?? ServerTryCreateLootContainerInternal(character, tile, ensureTheHeightIsTheSame: false);
        }

        public override string ClientGetTitle(IStaticWorldObject worldObject)
        {
            var ownerName = GetPublicState(worldObject).OwnerName;
            if (ownerName == ClientCurrentCharacterHelper.Character?.Name)
            {
                return MessageLootFromCurrentPlayer;
            }

            return string.Format(MessageFormatLootFromAnotherPlayer, ownerName);
        }

        public BaseUserControlWithWindow ClientOpenUI(IStaticWorldObject worldObject)
        {
            var itemsContainer = GetPrivateState(worldObject).ItemsContainer;
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
            return isCanInteract ? CursorId.PickupPossible : CursorId.PickupImpossible;
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            // don't call base implementation
            //base.ServerOnDestroy(gameObject);

            var lootObjectPrivateState = GetPrivateState(gameObject);
            var owner = lootObjectPrivateState.Owner;
            if (owner == null)
            {
                return;
            }

            // remove map from mark
            var characterPrivateState = PlayerCharacter.GetPrivateState(owner);
            characterPrivateState.DroppedItemsLocations
                                 .Remove(gameObject.TilePosition);

            var lastInteractingCharacter = lootObjectPrivateState.LastInteractingCharacter;
            if (owner == lastInteractingCharacter
                || lastInteractingCharacter == null)
            {
                // owner looted it or it's disappeared by timeout
                return;
            }

            // someone looted the owner items
            this.CallClient(owner,
                            _ => _.ClientRemote_NotifyLootFinished(lastInteractingCharacter.Name));
        }

        public void ServerOnMenuClosed(ICharacter who, IStaticWorldObject worldObject)
        {
            // nothing here
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
            => (0.5, 0.15);

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

        void IInteractableProtoStaticWorldObject.ServerOnClientInteract(ICharacter who, IStaticWorldObject worldObject)
        {
            Logger.Important($"{who} interacting with {worldObject}");
            var privateState = GetPrivateState(worldObject);
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

            var sceneObject = Client.Scene.GetSceneObject(data.GameObject);
            Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                this.DefaultTexture,
                drawOrder: DrawOrder.Default,
                positionOffset: (0.5, 0.5),
                spritePivotPoint: (0.5, 0.5),
                scale: 0.5);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableStaticWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected override void ClientOnObjectDestroyed(Vector2Ushort tilePosition)
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
                .Add(new ConstructionTileRequirements.Validator(ConstructionTileRequirements.ErrorNoFreeSpace,
                                                                c => c.Tile.StaticObjects.All(
                                                                    _ => _.ProtoStaticWorldObject.Kind
                                                                         == StaticObjectKind.Floor)))
                // ensure no static physical objects at tile
                .Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyStatic)
                // ensure no other static objects (including loot)
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

            if (itemsContainer == null)
            {
                // create container
                itemsContainer = ServerItemsService.CreateContainer<ItemsContainerOutputPublic>(
                    data.GameObject,
                    slotsCount: 1);
                privateState.ItemsContainer = itemsContainer;
            }
            else if (!(itemsContainer.ProtoItemsContainer is ItemsContainerOutputPublic))
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
            if (instance.CheckTileRequirements(tilePosition, character: null, logErrors: false))
            {
                Logger.Important("Creating loot container at " + tilePosition);
                objectLootContainer = Server.World.CreateStaticWorldObject(instance, tilePosition);
            }

            if (objectLootContainer == null)
            {
                // cannot create loot container
                if (writeWarningsToLog)
                {
                    Logger.Warning(
                        $"Cannot create loot container at {tilePosition} - tile contains something preventing it.");
                }

                return null;
            }

            GetPrivateState(objectLootContainer).Owner = character;
            GetPublicState(objectLootContainer).OwnerName = character.Name;

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            characterPrivateState.DroppedItemsLocations.AddIfNotContains(tilePosition);

            return GetPrivateState(objectLootContainer).ItemsContainer;
        }

        // try create the loot container nearby the character death position
        private static IItemsContainer ServerTryCreateLootContainerInternal(
            ICharacter character,
            Tile startTile,
            bool ensureTheHeightIsTheSame)
        {
            var startTileHeight = startTile.Height;

            var checkQueue = new List<Tile>();
            checkQueue.Add(startTile);

            var checkedTiles = new HashSet<Vector2Ushort>();
            checkedTiles.Add(startTile.Position);

            // try to drop the loot at max 10 tiles away from the requested tile
            const byte maxNeighborFindIterations = 10;
            var iterationAttempt = 0;

            while (true)
            {
                iterationAttempt++;

                foreach (var tile in checkQueue)
                {
                    if (!tile.IsValidTile)
                    {
                        continue;
                    }

                    if (ensureTheHeightIsTheSame
                        && tile.Height != startTileHeight)
                    {
                        // different tile height
                        continue;
                    }

                    var lootContainer = ServerTryCreateLootContainerAtPosition(
                        tile.Position,
                        character,
                        writeWarningsToLog: false);
                    if (lootContainer != null)
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
                // enqueue all the neighbor tiles
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

                // randomize all the neighbors which will be checked on the next iteration
                checkQueue.Shuffle();
            }

            // helper method to enqueue not yet checked tiles from the tile neighbors
            void EnqueueNeighbors(Tile tile)
            {
                var neighborTiles = tile.EightNeighborTiles;
                foreach (var neighborTile in neighborTiles)
                {
                    if (!checkedTiles.Add(neighborTile.Position))
                    {
                        // the tile is already checked
                        continue;
                    }

                    checkQueue.Add(neighborTile);
                }
            }
        }

        private void ClientRemote_NotifyLootFinished(string name)
        {
            NotificationSystem.ClientShowNotification(
                NotificationCurrentPlayerItemsTaken_Title,
                string.Format(NotificationCurrentPlayerItemsTaken_Message, name),
                NotificationColor.Bad,
                this.DefaultTexture,
                autoHide: true);
        }

        private void ClientRemote_NotifyLootInteraction(string name)
        {
            NotificationSystem.ClientShowNotification(
                NotificationCurrentPlayerItemsBeingLooted_Title,
                string.Format(NotificationCurrentPlayerItemsBeingLooted_Message, name),
                NotificationColor.Bad,
                this.DefaultTexture,
                autoHide: true);
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