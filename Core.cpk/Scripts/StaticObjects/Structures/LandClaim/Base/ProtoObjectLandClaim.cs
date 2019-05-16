namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.LandClaims;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.LandClaims.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectLandClaim
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectLandClaim,
          IInteractableProtoStaticWorldObject
        where TPrivateState : ObjectLandClaimPrivateState, new()
        where TPublicState : ObjectLandClaimPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public const string DialogCannotUpgrade = "Cannot upgrade";

        public const string NotificationDontHaveAccess = "You don't have access to this land claim.";

        public const string NotificationUpgraded_Message = "Land claim structure has been upgraded.";

        public const string NotificationUpgraded_Title = "Successfully upgraded";

        private static readonly Color BlueprintAreaColorGrace = Color.FromArgb(0x33, 0x22, 0x22, 0x22);

        private static readonly Color BlueprintAreaColorGreen = Color.FromArgb(0x44, 0x00, 0xFF, 0x00);

        private static readonly Color BlueprintAreaColorRed = Color.FromArgb(0x66, 0xDD, 0x00, 0x00);

        // ReSharper disable once StaticMemberInGenericType
        private static readonly RenderingMaterial ClientBlueprintAreaRenderingMaterial;

        private static readonly RenderingMaterial ClientBlueprintGraceAreaRenderingMaterial;

        private static readonly RenderingMaterial ClientBlueprintRestrictedTileRenderingMaterial;

        // how long the items dropped on the ground from the safe storage should remain there
        private static readonly TimeSpan DestroyedLandClaimDroppedItemsDestructionTimeout = TimeSpan.FromDays(1);

        private readonly Lazy<ushort> lazyLandClaimGraceAreaPaddingSizeOneDirection;

        private IComponentAttachedControl currentDisplayedTooltip;

        static ProtoObjectLandClaim()
        {
            if (IsClient)
            {
                // prepare material and effect parameters
                ClientBlueprintAreaRenderingMaterial = ClientLandClaimAreaRenderer.CreateRenderingMaterial();
                ClientBlueprintGraceAreaRenderingMaterial = ClientLandClaimAreaRenderer.CreateRenderingMaterial();
                ClientBlueprintGraceAreaRenderingMaterial.EffectParameters.Set("Color", BlueprintAreaColorGrace);

                ClientBlueprintRestrictedTileRenderingMaterial = ClientLandClaimAreaRenderer.CreateRenderingMaterial();
                ClientBlueprintRestrictedTileRenderingMaterial.EffectParameters.Set("Color", BlueprintAreaColorRed);
            }
        }

        protected ProtoObjectLandClaim()
        {
            // no problem with that call as it's accessing a simple number
            // ReSharper disable once VirtualMemberCallInConstructor
            this.lazyLandClaimGraceAreaPaddingSizeOneDirection = new Lazy<ushort>(
                () => LandClaimSystem.SharedCalculateLandClaimGraceAreaPaddingSizeOneDirection(
                    this.LandClaimSize));
        }

        public abstract TimeSpan DestructionTimeout { get; }

        public override string InteractionTooltipText => InteractionTooltipTexts.Configure;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public ushort LandClaimGraceAreaPaddingSizeOneDirection
            => this.lazyLandClaimGraceAreaPaddingSizeOneDirection.Value;

        public abstract ushort LandClaimSize { get; }

        public ushort LandClaimWithGraceAreaSize
            => (ushort)(this.LandClaimSize
                        + 2 * this.LandClaimGraceAreaPaddingSizeOneDirection);

        public abstract byte SafeItemsSlotsCount { get; }

        public virtual ITextureResource TextureResourceObjectBroken { get; protected set; }

        public BaseUserControlWithWindow ClientOpenUI(IStaticWorldObject worldObject)
        {
            return this.ClientOpenUI(new ClientObjectData(worldObject));
        }

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            base.ClientSetupBlueprint(tile, blueprint);

            if (!ConstructionPlacementSystem.IsObjectPlacementComponentEnabled)
            {
                return;
            }

            // display area only when player placing a blueprint
            Color color;
            var sizeWithGraceArea = this.LandClaimWithGraceAreaSize;
            if (blueprint.IsCanBuild)
            {
                color = BlueprintAreaColorGreen;
                ClientBlueprintAreaRenderingMaterial.EffectParameters.Set("Color", color);
                CreateBlueprintAreaRenderer(ClientBlueprintAreaRenderingMaterial,      this.LandClaimSize);
                CreateBlueprintAreaRenderer(ClientBlueprintGraceAreaRenderingMaterial, sizeWithGraceArea);

                // can build a land claim area, but highlight the restricted tiles
                var world = Client.World;
                var startTilePosition = tile.Position;
                var halfSize = sizeWithGraceArea / 2;
                for (var x = 1 - halfSize; x <= halfSize; x++)
                for (var y = 1 - halfSize; y <= halfSize; y++)
                {
                    var checkTile = world.GetTile(startTilePosition.X + x,
                                                  startTilePosition.Y + y,
                                                  logOutOfBounds: false);
                    ProcessFutureLandClaimAreaTile(checkTile, x, y);
                }
            }
            else // cannot build a land claim there
            {
                color = BlueprintAreaColorRed;
                ClientBlueprintAreaRenderingMaterial.EffectParameters.Set("Color", color);
                CreateBlueprintAreaRenderer(ClientBlueprintAreaRenderingMaterial,
                                            size: sizeWithGraceArea);
            }

            void CreateBlueprintAreaRenderer(RenderingMaterial renderingMaterial, int size)
            {
                var offset = LandClaimSystem.SharedCalculateLandClaimObjectCenterTilePosition(Vector2Ushort.Zero, this);
                var areaBlueprint = Api.Client.Rendering.CreateSpriteRenderer(
                    blueprint.SceneObject,
                    null,
                    DrawOrder.Overlay);

                areaBlueprint.RenderingMaterial = renderingMaterial;
                areaBlueprint.SortByWorldPosition = false;
                areaBlueprint.IgnoreTextureQualityScaling = true;
                areaBlueprint.Scale = size;

                areaBlueprint.PositionOffset = (offset.X - size / 2,
                                                offset.Y - size / 2);
            }

            void ProcessFutureLandClaimAreaTile(Tile checkTile, int offsetX, int offsetY)
            {
                if (!checkTile.IsValidTile)
                {
                    return;
                }

                var isRestrictedTile = IsRestrictedTile(checkTile);
                if (!isRestrictedTile)
                {
                    foreach (var neighborTile in checkTile.EightNeighborTiles)
                    {
                        if (IsRestrictedTile(neighborTile))
                        {
                            isRestrictedTile = true;
                            break;
                        }
                    }
                }

                if (!isRestrictedTile)
                {
                    return;
                }

                // display red tile as player cannot construct there
                var tileRenderer = Api.Client.Rendering.CreateSpriteRenderer(
                    blueprint.SceneObject,
                    ClientLandClaimAreaRenderer.TextureResourceLandClaimAreaCell,
                    DrawOrder.Overlay);

                tileRenderer.RenderingMaterial = ClientBlueprintRestrictedTileRenderingMaterial;
                tileRenderer.SortByWorldPosition = false;
                tileRenderer.IgnoreTextureQualityScaling = true;
                tileRenderer.Scale = 1;
                tileRenderer.PositionOffset = (offsetX, offsetY);
            }

            bool IsRestrictedTile(Tile t)
                => t.IsCliffOrSlope
                   || t.ProtoTile.Kind != TileKind.Solid
                   || t.ProtoTile.IsRestrictingConstruction;
        }

        public async void ClientUpgrade(IStaticWorldObject worldObjectLandClaim, IProtoObjectLandClaim upgradeStructure)
        {
            var checkResult = this.SharedCanUpgrade(
                worldObjectLandClaim,
                upgradeStructure,
                Client.Characters.CurrentPlayerCharacter,
                out _);

            if (checkResult == ObjectLandClaimCanUpgradeCheckResult.Success)
            {
                // client check successful - send the upgrade request to server
                checkResult = await this.CallServer(
                                  _ => _.ServerRemote_UpgradeStructure(worldObjectLandClaim, upgradeStructure));
                // process the upgrade result from server
                if (checkResult == ObjectLandClaimCanUpgradeCheckResult.Success)
                {
                    NotificationSystem.ClientShowNotification(
                        NotificationUpgraded_Title,
                        NotificationUpgraded_Message,
                        color: NotificationColor.Good,
                        icon: this.Icon);
                    return;
                }
            }

            DialogWindow.ShowMessage(DialogCannotUpgrade,
                                     text: checkResult.GetDescription(),
                                     closeByEscapeKey: true);
        }

        public sealed override void ServerOnBuilt(IStaticWorldObject structure, ICharacter byCharacter)
        {
            base.ServerOnBuilt(structure, byCharacter);
            LandClaimSystem.ServerOnObjectLandClaimBuilt(byCharacter, structure);
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            LandClaimSystem.ServerOnObjectLandClaimDestroyed(gameObject);

            // try drop items from the safe storage
            var itemsContainer = GetPrivateState(gameObject).ItemsContainer;
            if (itemsContainer.OccupiedSlotsCount == 0)
            {
                // no items to drop
                return;
            }

            var groundContainer = ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                itemsContainer);

            if (groundContainer == null)
            {
                // no items dropped
                return;
            }

            // set custom timeout for the dropped ground items container
            ObjectGroundItemsContainer.ServerSetDestructionTimeout(
                (IStaticWorldObject)groundContainer.Owner,
                DestroyedLandClaimDroppedItemsDestructionTimeout.TotalSeconds);
        }

        public ObjectLandClaimCanUpgradeCheckResult ServerRemote_UpgradeStructure(
            IStaticWorldObject oldWorldObjectLandClaim,
            IProtoObjectLandClaim upgradeStructure)
        {
            this.VerifyGameObject(oldWorldObjectLandClaim);
            var character = ServerRemoteContext.Character;
            var result = this.SharedCanUpgrade(oldWorldObjectLandClaim,
                                               upgradeStructure,
                                               character,
                                               out var upgradeEntry);
            if (result != ObjectLandClaimCanUpgradeCheckResult.Success)
            {
                return result;
            }

            // consume items
            upgradeEntry.ServerDestroyRequiredItems(character);

            // copy all items to temp container
            var oldStorage = GetPrivateState(oldWorldObjectLandClaim).ItemsContainer;

            var tempStorageOwner = Server.World.CreateLogicObject<LogicObjectTempItemsContainerHolder>();
            var tempStorage = Server.Items.CreateContainer(
                owner: tempStorageOwner, // we must set an owner, unfortunately
                slotsCount: (byte)oldStorage.OccupiedSlotsCount);

            Server.Items.TryMoveAllItems(oldStorage, tempStorage);

            // upgrade (it will destroy an existing structure and place new in its place)
            var upgradedWorldObjectLandClaim = LandClaimSystem.ServerUpgrade(oldWorldObjectLandClaim,
                                                                             upgradeStructure,
                                                                             character);

            // move all items from temp container to the upgraded land claim
            var newStorage = GetPrivateState(upgradedWorldObjectLandClaim).ItemsContainer;
            Server.Items.TryMoveAllItems(tempStorage, newStorage);
            Server.Items.DestroyContainer(tempStorage);
            Server.World.DestroyObject(tempStorageOwner);

            // notify client (to play sound)
            ConstructionPlacementSystem.Instance.ServerOnStructurePlaced(upgradedWorldObjectLandClaim, character);

            return result;
        }

        public bool SharedCanEditOwners(IStaticWorldObject worldObject, ICharacter byOwner)
        {
            var area = GetPublicState(worldObject).LandClaimAreaObject;
            var privateState = LandClaimArea.GetPrivateState(area);

            if (privateState.LandClaimFounder == byOwner.Name
                || CreativeModeSystem.SharedIsInCreativeMode(byOwner))
            {
                // only founder or character in creative mode can edit the owners list
                return true;
            }

            return false;
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            if (IsClient)
            {
                // cannot perform further checks on client side
                return true;
            }

            var publicState = GetPublicState(worldObject);
            if (LandClaimSystem.ServerIsOwnedArea(publicState.LandClaimAreaObject, character))
            {
                return true;
            }

            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            // not the land owner
            if (writeToLog)
            {
                Logger.Warning(
                    $"Character cannot interact with {worldObject} - not the land owner.",
                    character);

                this.CallClient(
                    character,
                    _ => _.ClientRemote_OnCannotInteract(
                        worldObject,
                        LandClaimMenuOpenResult.FailPlayerIsNotOwner));
            }

            return false;
        }

        public ObjectLandClaimCanUpgradeCheckResult SharedCanUpgrade(
            IStaticWorldObject worldObjectLandClaim,
            IProtoObjectLandClaim protoUpgradedLandClaim,
            ICharacter character,
            out IConstructionUpgradeEntryReadOnly upgradeEntry,
            bool writeErrors = true)
        {
            if (!this.SharedCanInteract(character,
                                        worldObjectLandClaim,
                                        writeToLog: writeErrors))
            {
                upgradeEntry = null;
                return ObjectLandClaimCanUpgradeCheckResult.ErrorUnknown;
            }

            upgradeEntry = null;
            foreach (var entry in this.ConfigUpgrade.Entries)
            {
                if (entry.ProtoStructure == protoUpgradedLandClaim)
                {
                    upgradeEntry = entry;
                    break;
                }
            }

            var currentLandClaimArea = GetPublicState(worldObjectLandClaim).LandClaimAreaObject;
            var founderName = LandClaimArea.GetPrivateState(currentLandClaimArea).LandClaimFounder;

            var result = ObjectLandClaimCanUpgradeCheckResult.Success;
            if (upgradeEntry == null)
            {
                result = ObjectLandClaimCanUpgradeCheckResult.ErrorUnknown;
            }

            if (result == ObjectLandClaimCanUpgradeCheckResult.Success)
            {
                if (character.Name != founderName)
                {
                    result = ObjectLandClaimCanUpgradeCheckResult.ErrorNotFounder;
                }
            }

            if (result == ObjectLandClaimCanUpgradeCheckResult.Success)
            {
                // validate player know the tech, have enough items, etc
                if (!upgradeEntry.CheckRequirementsSatisfied(character))
                {
                    upgradeEntry = null;
                    result = ObjectLandClaimCanUpgradeCheckResult.ErrorRequirementsNotSatisfied;
                }
            }

            if (result == ObjectLandClaimCanUpgradeCheckResult.Success)
            {
                // check there will be no intersection with other areas
                if (!LandClaimSystem.SharedCheckCanPlaceOrUpgradeLandClaimThere(protoUpgradedLandClaim,
                                                                                worldObjectLandClaim.TilePosition,
                                                                                character))
                {
                    result = ObjectLandClaimCanUpgradeCheckResult.ErrorAreaIntersection;
                }
            }

            if (result == ObjectLandClaimCanUpgradeCheckResult.Success)
            {
                if (!InteractionCheckerSystem.HasInteraction(character,
                                                             worldObjectLandClaim,
                                                             requirePrivateScope: true))
                {
                    result = ObjectLandClaimCanUpgradeCheckResult.ErrorNoActiveInteraction;
                }
            }

            if (writeErrors
                && result != ObjectLandClaimCanUpgradeCheckResult.Success)
            {
                Logger.Warning(
                    $"Can\'t upgrade: {worldObjectLandClaim} to {protoUpgradedLandClaim}: error code - {result}",
                    character);
            }

            return result;
        }

        void IInteractableProtoStaticWorldObject.ServerOnClientInteract(ICharacter who, IStaticWorldObject worldObject)
        {
            if (!CreativeModeSystem.SharedIsInCreativeMode(who))
            {
                return;
            }

            // ensure that the area is in the private scope of the creative mode player
            var area = LandClaimSystem.ServerGetLandClaimArea(worldObject);
            if (area == null)
            {
                // area could be null in the Editor for the land claim without owners
                return;
            }

            Server.World.EnterPrivateScope(who, area);
        }

        void IInteractableProtoStaticWorldObject.ServerOnMenuClosed(ICharacter who, IStaticWorldObject worldObject)
        {
            // do nothing
        }

        protected abstract BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject);

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;
            var publicState = data.SyncPublicState;
            var clientState = data.ClientState;

            data.ClientState.RendererLight = this.ClientCreateLightSource(
                Client.Scene.GetSceneObject(worldObject));

            // subscribe to destruction timer
            publicState.ClientSubscribe(
                _ => _.ServerTimeForDestruction,
                _ => ClientRefreshSprite(),
                clientState);

            ClientRefreshSprite();

            void ClientRefreshSprite()
            {
                clientState.Renderer.TextureResource = publicState.ServerTimeForDestruction.HasValue
                                                           ? this.TextureResourceObjectBroken
                                                           : this.DefaultTexture;
            }
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableStaticWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected override void ClientObserving(ClientObjectData data, bool isObserving)
        {
            base.ClientObserving(data, isObserving);

            if (!isObserving)
            {
                this.currentDisplayedTooltip.Destroy();
                this.currentDisplayedTooltip = null;
                return;
            }

            var worldObject = data.GameObject;
            var control = new BrokenObjectLandClaimTooltip()
            {
                ObjectLandClaim = worldObject,
                ObjectLandClaimPublicState = data.SyncPublicState,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            this.currentDisplayedTooltip = Client.UI.AttachControl(
                worldObject,
                control,
                positionOffset: this.SharedGetObjectCenterWorldOffset(
                                    worldObject)
                                + (0, 0.55),
                isFocusable: false);
        }

        protected BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowLandClaim.Open(
                new ViewModelWindowLandClaim(data.GameObject, data.SyncPublicState.LandClaimAreaObject));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            renderer.SpritePivotPoint = (0.5, 0);
            renderer.PositionOffset = (1, 0.5);
            renderer.DrawOrderOffsetY = 0.25;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected sealed override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            tileRequirements.Add(LandClaimSystem.ValidatorNewLandClaimNoLandClaimIntersections);
            tileRequirements.Add(LandClaimSystem.ValidatorCheckCharacterLandClaimAmountLimit);
            tileRequirements.Add(LandClaimSystem.ValidatorCheckCharacterLandClaimDepositRequireXenogeology);
            tileRequirements.Add(LandClaimSystem.ValidatorCheckCharacterLandClaimDepositCooldown);
            this.PrepareLandClaimConstructionConfig(tileRequirements, build, repair, upgrade, out category);

            var landClaimSize = this.LandClaimSize;
            Api.Assert(landClaimSize % 2 == 0, "Land claim size should be an even number");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var texturePath = GenerateTexturePath(thisType);
            this.TextureResourceObjectBroken = new TextureResource(texturePath + "Broken");
            return new TextureResource(texturePath);
        }

        protected abstract void PrepareLandClaimConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category);

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var itemsContainer = data.PrivateState.ItemsContainer;
            if (itemsContainer != null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: this.SafeItemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer(
                owner: data.GameObject,
                slotsCount: this.SafeItemsSlotsCount);

            data.PrivateState.ItemsContainer = itemsContainer;
        }

        protected override void ServerOnStaticObjectDamageApplied(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            float previousStructurePoints,
            float currentStructurePoints)
        {
            base.ServerOnStaticObjectDamageApplied(weaponCache,
                                                   targetObject,
                                                   previousStructurePoints,
                                                   currentStructurePoints);

            if (weaponCache != null
                && currentStructurePoints < previousStructurePoints)
            {
                // land claim was damaged (and not deconstructed by a crowbar or any other means)
                LandClaimSystem.ServerOnRaid(targetObject.Bounds,
                                             weaponCache.Character);
            }
        }

        protected override void ServerOnStaticObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            // do not use default implementation because it will destroy the object automatically
            //base.ServerOnStaticObjectZeroStructurePoints(weaponCache, targetObject);

            var publicState = GetPublicState((IStaticWorldObject)targetObject);
            if (byCharacter != null
                && (LandClaimSystem.ServerIsOwnedArea(publicState.LandClaimAreaObject, byCharacter)
                    || CreativeModeSystem.SharedIsInCreativeMode(byCharacter)))
            {
                // this is the owner of the area or the player is in a creative mode
                if (byCharacter.SharedGetPlayerSelectedHotbarItemProto() is ProtoItemToolCrowbar)
                {
                    publicState.ServerTimeForDestruction = 0;
                    Logger.Important(
                        $"Land claim object {targetObject} destroyed by the owner with a crowbar - no destruction timer",
                        byCharacter);
                    return;
                }
            }

            if (publicState.ServerTimeForDestruction.HasValue)
            {
                // destruction timer is already set
                return;
            }

            // the land claim structure points is zero - it's broken now - set timer for destruction
            publicState.ServerTimeForDestruction = Server.Game.FrameTime
                                                   + this.DestructionTimeout.TotalSeconds;
            Logger.Important($"Timer for destruction set: {targetObject}. Timeout: {this.DestructionTimeout}");
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var publicState = data.PublicState;
            if (!publicState.ServerTimeForDestruction.HasValue)
            {
                return;
            }

            var gameObject = data.GameObject;
            if (publicState.StructurePointsCurrent >= this.StructurePointsMax)
            {
                // the broken land claim structure repaired completely - remove timer for destruction
                publicState.ServerTimeForDestruction = null;
                Logger.Important("Timer for destruction removed: " + gameObject);
                return;
            }

            if (Server.Game.FrameTime < publicState.ServerTimeForDestruction.Value)
            {
                return;
            }

            // time to destroy
            Logger.Important("Destroying object after timeout: " + gameObject);
            this.ServerSendObjectDestroyedEvent(gameObject);
            Server.World.DestroyObject(gameObject);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            const double width = 1.5,
                         height = 1.1,
                         offsetX = (2 - width) / 2,
                         offsetY = 0.6;

            data.PhysicsBody
                .AddShapeRectangle((width, height),
                                   offset: (offsetX, offsetY),
                                   group: CollisionGroups.Default)
                .AddShapeRectangle((width, height),
                                   offset: (offsetX, offsetY),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((width, height),
                                   offset: (offsetX, offsetY),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((1.2, height + 0.2),
                                   offset: (0.4, offsetY),
                                   group: CollisionGroups.ClickArea);
        }

        private void ClientRemote_OnCannotInteract(IStaticWorldObject worldObject, LandClaimMenuOpenResult result)
        {
            if (result == LandClaimMenuOpenResult.FailPlayerIsNotOwner)
            {
                this.ClientOnCannotInteract(worldObject,
                                            NotificationDontHaveAccess,
                                            isOutOfRange: false);
                return;
            }

            Logger.Warning($"Received cannot open land menu result: {worldObject},  result={result}");
        }
    }

    public abstract class ProtoObjectLandClaim
        : ProtoObjectLandClaim
            <ObjectLandClaimPrivateState,
                ObjectLandClaimPublicState,
                StaticObjectClientState>
    {
    }
}