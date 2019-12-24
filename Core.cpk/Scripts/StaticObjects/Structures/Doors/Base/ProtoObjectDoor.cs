namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Walls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectDoor
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectDoor
        where TPrivateState : ObjectDoorPrivateState, new()
        where TPublicState : ObjectDoorPublicState, new()
        where TClientState : ObjectDoorClientState, new()
    {
        private const double DrawWorldOffsetYHorizontalDoor = 0.17;

        private const double DrawWorldOffsetYVerticalDoor = 0.1;

        private const double HorizontalDoorPhysicsHeight = 0.5;

        private const double VerticalDoorPhysicsWidthOpened = 0.25;
        
        // Workaround: using the same width as for opened door to prevent issue with the door opening-closing in cycle
        // due to a change of direct line of sight when door width changed.
        private const double VerticalDoorPhysicsWidthClosed = VerticalDoorPhysicsWidthOpened;

        private const double VerticalDoorPhysicsWidthOpenedOffsetX = 0.5 - VerticalDoorPhysicsWidthOpened * 0.5;

        private const double VerticalDoorPhysicsWidthClosedOffsetX = 0.5 - VerticalDoorPhysicsWidthClosed * 0.5;

        private readonly Lazy<ProceduralTexture> lazyHorizontalDoorBlueprintTexture;

        private readonly Lazy<ProceduralTexture> lazyVerticalDoorBlueprintTexture;

        private BoundsDouble doorOpenCheckBoundsHorizontal;

        private BoundsDouble doorOpenCheckBoundsVertical;

        protected ProtoObjectDoor()
        {
            this.lazyHorizontalDoorBlueprintTexture = new Lazy<ProceduralTexture>(
                () =>
                {
                    var textureResources = new[]
                    {
                        this.TextureBaseHorizontal,
                        (ITextureResource)this.AtlasTextureHorizontal.Chunk(
                            (byte)(this.AtlasTextureHorizontal.AtlasSize.ColumnsCount - 1),
                            0)
                    };
                    var proceduralTexture = new ProceduralTexture(
                        "Composed blueprint " + this.Id,
                        generateTextureCallback: request => ClientComposeHorizontalDoor(request,
                                                                                        textureResources),
                        isTransparent: true,
                        isUseCache: true,
                        dependsOn: textureResources);
                    return proceduralTexture;
                });

            this.lazyVerticalDoorBlueprintTexture = new Lazy<ProceduralTexture>(
                () =>
                {
                    var atlas = this.AtlasTextureVertical;
                    var columnChunk = (byte)(atlas.AtlasSize.ColumnsCount - 1);
                    return ClientProceduralTextureHelper.CreateComposedTexture(
                        "Composed vertical door blueprint " + this.Id,
                        isTransparent: true,
                        isUseCache: true,
                        textureResources: new ITextureResource[]
                        {
                            atlas.Chunk(columnChunk,             0), // door base
                            atlas.Chunk((byte)(columnChunk - 1), 0), // door front part
                            atlas.Chunk((byte)(columnChunk - 2), 0)  // door back part (opened)
                        });
                });
        }

        public virtual int DoorSizeTiles => 1;

        public virtual bool HasOwnersList => true;

        public override string InteractionTooltipText => InteractionTooltipTexts.Configure;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public bool IsClosedAccessModeAvailable => true;

        public virtual bool IsHeavyVehicleCanPass => false;

        /// <summary>
        /// If set to null the door orientation is selected automatically.
        /// </summary>
        public virtual bool? IsHorizontalDoorOnly => null;

        public override double ServerUpdateIntervalSeconds => 0.2; // 5 times per second

        public virtual SoundResource SoundResourceDoorEnd { get; }
            = new SoundResource("Objects/Structures/Doors/DoorEnd");

        public virtual SoundResource SoundResourceDoorProcess { get; }
            = new SoundResource("Objects/Structures/Doors/DoorProcess");

        public virtual SoundResource SoundResourceDoorStart { get; }
            = new SoundResource("Objects/Structures/Doors/DoorStart");

        public override float StructurePointsMaxForConstructionSite
            => this.StructurePointsMax / 25;

        protected ITextureAtlasResource AtlasTextureHorizontal { get; set; }

        protected ITextureAtlasResource AtlasTextureVertical { get; set; }

        protected virtual double DoorOpenCloseAnimationDuration => 0.2;

        // Large bases have multiple doors so making them a little quiet is a good idea.
        protected virtual float SoundsVolume => 0.5f;

        protected TextureResource TextureBaseHorizontal { get; set; }

        public void ClientRefreshRenderer(IStaticWorldObject door)
        {
            this.ClientSetupDoor(new ClientInitializeData(door));
        }

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            var renderer = blueprint.SpriteRenderer;
            var isHorizontalDoor = this.IsHorizontalDoorOnly
                                   ?? DoorHelper.IsHorizontalDoorNeeded(tile, checkExistingDoor: true);
            renderer.PositionOffset = (0,
                                       isHorizontalDoor
                                           ? DrawWorldOffsetYHorizontalDoor
                                           : DrawWorldOffsetYVerticalDoor);

            renderer.TextureResource = isHorizontalDoor
                                           ? this.lazyHorizontalDoorBlueprintTexture.Value
                                           : this.lazyVerticalDoorBlueprintTexture.Value;
        }

        public override void ServerOnBuilt(IStaticWorldObject structure, ICharacter byCharacter)
        {
            WorldObjectOwnersSystem.ServerOnBuilt(structure, byCharacter);
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            foreach (var occupiedTile in gameObject.OccupiedTiles)
            {
                SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(occupiedTile,
                                                                                 isDestroy: true);
            }

            base.ServerOnDestroy(gameObject);
        }

        public bool SharedCanEditOwners(IWorldObject worldObject, ICharacter byOwner)
        {
            return true;
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            return base.SharedCanInteract(character, worldObject, writeToLog)
                   && WorldObjectOwnersSystem.SharedCanInteract(character, worldObject, writeToLog);
        }

        public override void SharedCreatePhysicsConstructionBlueprint(IPhysicsBody physicsBody)
        {
            var worldObject = (IStaticWorldObject)physicsBody.AssociatedWorldObject;
            var isHorizontalDoor = this.IsHorizontalDoorOnly
                                   ?? DoorHelper.IsHorizontalDoorNeeded(worldObject.OccupiedTile,
                                                                        checkExistingDoor: true);
            this.SharedCreateDoorPhysics(physicsBody, isHorizontalDoor, isOpened: true);
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            return this.ClientOpenUI(new ClientObjectData((IStaticWorldObject)worldObject));
        }

        ObjectDoorPrivateState IProtoObjectDoor.GetPrivateState(IStaticWorldObject door)
        {
            return GetPrivateState(door);
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        protected static async Task<ITextureResource> ClientComposeHorizontalDoor(
            ProceduralTextureRequest request,
            params ITextureResource[] textureResources)
        {
            var rendering = Api.Client.Rendering;
            var renderingTag = request.TextureName;

            var textureSize = await rendering.GetTextureSize(textureResources[1]);
            var textureSizeBase = await rendering.GetTextureSize(textureResources[0]);
            request.ThrowIfCancelled();

            // create camera and render texture
            var renderTexture = rendering.CreateRenderTexture(renderingTag, textureSize.X, textureSize.Y);
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = rendering.CreateCamera(cameraObject,
                                                renderingTag,
                                                drawOrder: -100);
            camera.RenderTarget = renderTexture;
            camera.SetOrthographicProjection(textureSize.X, textureSize.Y);

            // create and prepare sprite renderers
            rendering.CreateSpriteRenderer(
                cameraObject,
                textureResources[0],
                positionOffset: (0, -textureSize.Y + textureSizeBase.Y),
                // draw down
                spritePivotPoint: (0, 1),
                renderingTag: renderingTag);

            rendering.CreateSpriteRenderer(
                cameraObject,
                textureResources[1],
                positionOffset: (0, 0),
                // draw down
                spritePivotPoint: (0, 1),
                renderingTag: renderingTag);

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: true);
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }

        protected override ITextureResource ClientCreateIcon()
        {
            if (this.IsHorizontalDoorOnly ?? true)
            {
                var textureResources = new ITextureResource[]
                {
                    // closed door
                    this.TextureBaseHorizontal,
                    // horizontal base
                    this.AtlasTextureHorizontal.Chunk(0, 0)
                };

                return new ProceduralTexture(
                    "Composed " + this.Id,
                    generateTextureCallback: request => ClientComposeHorizontalDoor(request, textureResources),
                    isTransparent: true,
                    isUseCache: true,
                    dependsOn: textureResources);
            }

            // vertical door
            var atlas = this.AtlasTextureVertical;
            var columnChunk = (byte)(atlas.AtlasSize.ColumnsCount - 1);
            return ClientProceduralTextureHelper.CreateComposedTexture(
                "Composed vertical door blueprint " + this.Id,
                isTransparent: true,
                isUseCache: true,
                textureResources: new ITextureResource[]
                {
                    atlas.Chunk(columnChunk,             0), // door base
                    atlas.Chunk((byte)(columnChunk - 1), 0), // door front part
                    atlas.Chunk(0,                       0)  // door back part (closed)
                });
        }

        protected override void ClientDeinitializeStructure(IStaticWorldObject gameObject)
        {
            base.ClientDeinitializeStructure(gameObject);
            foreach (var occupiedTile in gameObject.OccupiedTiles)
            {
                SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(occupiedTile,
                                                                                 isDestroy: true);
            }
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            // don't use base implementation
            //base.ClientInitialize(data);

            this.ClientAddAutoStructurePointsBar(data);

            var publicState = data.PublicState;
            var clientState = data.ClientState;

            this.ClientSetupDoor(data);

            // subscribe on IsOpened change
            var staticWorldObject = data.GameObject;
            publicState.ClientSubscribe(
                _ => _.IsOpened,
                newIsOpened =>
                {
                    clientState.IsOpened = newIsOpened;
                    this.SharedCreatePhysics(staticWorldObject);
                    clientState.SpriteAnimator.Start(isPositiveDirection: newIsOpened);
                    Client.Audio.PlayOneShot(
                        this.SoundResourceDoorStart,
                        staticWorldObject,
                        volume: this.SoundsVolume);

                    // we don't use the looped "process" door sound
                    //Client.Audio.PlayOneShotLooped(
                    //    this.SoundResourceDoorProcess,
                    //    staticWorldObject,
                    //    duration: this.DoorOpenCloseAnimationDuration,
                    //    volume: SoundsVolume);

                    Client.Audio.PlayOneShot(
                        this.SoundResourceDoorEnd,
                        staticWorldObject,
                        delay: this.DoorOpenCloseAnimationDuration - this.DoorOpenCloseAnimationDuration / 5f,
                        volume: this.SoundsVolume);
                },
                subscriptionOwner: clientState);

            // subscribe on IsHorizontalDoor change
            publicState.ClientSubscribe(
                _ => _.IsHorizontalDoor,
                newIsHorizontal => this.ClientSetupDoor(data),
                subscriptionOwner: clientState);

            foreach (var occupiedTile in staticWorldObject.OccupiedTiles)
            {
                SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(occupiedTile,
                                                                                 isDestroy: false);
            }
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected override void ClientObserving(ClientObjectData data, bool isObserving)
        {
            base.ClientObserving(data, isObserving);
            StructureLandClaimIndicatorManager.ClientObserving(data.GameObject, isObserving);
        }

        protected BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowDoor.Open(
                new ViewModelWindowDoor(data.GameObject));
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryBuildings>();
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return this.AtlasTextureHorizontal;
        }

        protected virtual void PrepareOpeningBounds(out BoundsDouble horizontal, out BoundsDouble vertical)
        {
            var distance = 1.6;
            horizontal = vertical = new BoundsDouble(minX: -distance,
                                                     minY: -distance,
                                                     maxX: distance,
                                                     maxY: distance);
        }

        protected virtual void PrepareProtoDoor()
        {
        }

        protected sealed override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();
            this.PrepareOpeningBounds(out this.doorOpenCheckBoundsHorizontal,
                                      out this.doorOpenCheckBoundsVertical);
            this.PrepareProtoDoor();
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var worldObject = data.GameObject;
            var privateState = data.PrivateState;
            WorldObjectOwnersSystem.ServerInitialize(worldObject);

            if (!data.IsFirstTimeInit)
            {
                if (!this.HasOwnersList)
                {
                    privateState.AccessMode = WorldObjectAccessMode.OpensToEveryone;
                }

                return;
            }

            var publicState = data.PublicState;
            privateState.AccessMode = this.HasOwnersList
                                          ? WorldObjectAccessMode.OpensToObjectOwnersOrAreaOwners
                                          : WorldObjectAccessMode.OpensToEveryone;

            // refresh door type
            publicState.IsHorizontalDoor = this.IsHorizontalDoorOnly
                                           ?? DoorHelper.IsHorizontalDoorNeeded(worldObject.OccupiedTile,
                                                                                checkExistingDoor: false);
            publicState.IsOpened = true;

            foreach (var occupiedTile in worldObject.OccupiedTiles)
            {
                SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(occupiedTile,
                                                                                 isDestroy: false);
            }
        }

        protected override void ServerOnStaticObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            base.ServerOnStaticObjectZeroStructurePoints(weaponCache, byCharacter, targetObject);

            if (weaponCache != null)
            {
                // door was destroyed (and not deconstructed by a crowbar or any other means)
                LandClaimSystem.ServerOnRaid(((IStaticWorldObject)targetObject).Bounds,
                                             byCharacter);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var publicState = data.PublicState;
            var isOpened = publicState.IsOpened;
            var isShouldBeOpened = this.ServerCheckIsDoorShouldBeOpened(data.GameObject,
                                                                        data.PrivateState);
            if (isOpened == isShouldBeOpened)
            {
                return;
            }

            if (!isShouldBeOpened)
            {
                // check whether it's possible to close the door now (no obstacles)
                if (!this.ServerCanCloseDoor(data.GameObject, data.PublicState))
                {
                    return;
                }
            }

            isOpened = isShouldBeOpened;
            publicState.IsOpened = isOpened;
            // recreate physics
            this.SharedCreatePhysics(data.GameObject);
        }

        protected void SharedCreateDoorPhysics(IPhysicsBody physicsBody, bool isHorizontalDoor, bool isOpened)
        {
            double doorSize = this.DoorSizeTiles;

            // custom center offset is used for interaction zone raycasting
            physicsBody.SetCustomCenterOffset(
                isHorizontalDoor
                    ? (0.5 * doorSize, WallPatterns.PhysicsOffset + HorizontalDoorPhysicsHeight / 2)
                    : (0.5, 0.5 * doorSize));

            if (isHorizontalDoor)
            {
                // horizontal door physics
                if (isOpened)
                {
                    const double horizontalDoorBorderWidth = 0.125;
                    physicsBody.AddShapeRectangle(
                                   size: (horizontalDoorBorderWidth, HorizontalDoorPhysicsHeight),
                                   offset: (0, WallPatterns.PhysicsOffset))
                               .AddShapeRectangle(
                                   size: (horizontalDoorBorderWidth, HorizontalDoorPhysicsHeight),
                                   offset: (doorSize - horizontalDoorBorderWidth, WallPatterns.PhysicsOffset));
                }
                else
                {
                    physicsBody.AddShapeRectangle(
                        size: (doorWidth: doorSize, HorizontalDoorPhysicsHeight),
                        offset: (0, WallPatterns.PhysicsOffset));
                }

                if (!isOpened)
                {
                    // add closed horizontal door hitboxes
                    physicsBody.AddShapeRectangle(
                                   size: (doorWidth: doorSize, 0.25), // Y value same as for wall
                                   offset: (0, 0.75),                 // Y value same as for wall
                                   group: CollisionGroups.HitboxMelee)
                               .AddShapeRectangle(
                                   size: (doorWidth: doorSize, y: 0.57), // Y value same as for wall
                                   offset: (x: 0, y: 0.85),              // Y value same as for wall
                                   group: CollisionGroups.HitboxRanged);
                }

                // click area
                physicsBody.AddShapeRectangle(
                    size: (doorWidth: doorSize, 1),
                    offset: (0, WallPatterns.PhysicsOffset),
                    group: CollisionGroups.ClickArea);
                return;
            }

            // vertical door physics
            const double doorOpenedColliderHeight = 0.125;

            if (isOpened)
            {
                physicsBody.AddShapeRectangle(
                               size: (VerticalDoorPhysicsWidthOpened, doorOpenedColliderHeight),
                               offset: (VerticalDoorPhysicsWidthOpenedOffsetX, 0))
                           .AddShapeRectangle(
                               size: (VerticalDoorPhysicsWidthOpened, doorOpenedColliderHeight),
                               offset: (VerticalDoorPhysicsWidthOpenedOffsetX, doorSize - doorOpenedColliderHeight));
            }
            else
            {
                physicsBody.AddShapeRectangle(
                    size: (VerticalDoorPhysicsWidthClosed, doorWidth: doorSize),
                    offset: (VerticalDoorPhysicsWidthClosedOffsetX, 0));
            }

            if (!isOpened)
            {
                // add closed vertical door hitboxes (please note: we're using the collider width of the opened door)
                physicsBody.AddShapeRectangle(
                               size: (VerticalDoorPhysicsWidthOpened, doorSize + 0.76),
                               offset: (VerticalDoorPhysicsWidthOpenedOffsetX, 0),
                               group: CollisionGroups.HitboxMelee)
                           .AddShapeRectangle(
                               size: (VerticalDoorPhysicsWidthOpened, doorSize + 0.86),
                               offset: (VerticalDoorPhysicsWidthOpenedOffsetX, 0),
                               group: CollisionGroups.HitboxRanged);
            }

            // click area
            physicsBody.AddShapeRectangle(
                size: (doorWidth: 0.5, doorSize + 0.75),
                offset: (0.25, 0),
                group: CollisionGroups.ClickArea);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var publicState = data.PublicState;
            var isHorizontalDoor = publicState.IsHorizontalDoor;
            var isOpened = publicState.IsOpened;

            this.SharedCreateDoorPhysics(data.PhysicsBody, isHorizontalDoor, isOpened);
        }

        protected BoundsDouble SharedGetDoorOpenBounds(bool isHorizontalDoor)
        {
            return isHorizontalDoor
                       ? this.doorOpenCheckBoundsHorizontal
                       : this.doorOpenCheckBoundsVertical;
        }

        protected virtual BoundsDouble SharedGetDoorOpeningBounds(IStaticWorldObject worldObject)
        {
            var isHorizontalDoor = GetPublicState(worldObject).IsHorizontalDoor;
            var objectOpeningBounds = this.SharedGetDoorOpenBounds(isHorizontalDoor);
            var offset = isHorizontalDoor
                             ? new Vector2D(0.5, 0.1)
                             : new Vector2D(0.5, 0.5);

            var tilePosition = worldObject.TilePosition;
            var boundsOffset = objectOpeningBounds.Offset;
            return new BoundsDouble(
                new Vector2D(
                    boundsOffset.X + offset.X + tilePosition.X,
                    boundsOffset.Y + offset.Y + tilePosition.Y),
                objectOpeningBounds.Size);
        }

        private void ClientSetupDoor(ClientInitializeData data)
        {
            var sceneObject = data.GameObject.ClientSceneObject;
            var publicState = data.PublicState;
            var clientState = data.ClientState;
            var isHorizontalDoor = publicState.IsHorizontalDoor;
            var isOpened = publicState.IsOpened;

            var atlas = isHorizontalDoor
                            ? this.AtlasTextureHorizontal
                            : this.AtlasTextureVertical;
            var renderer = Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                atlas.Chunk(0, 0),
                drawOrder: DrawOrder.Default);

            renderer.PositionOffset = (0,
                                       isHorizontalDoor
                                           ? DrawWorldOffsetYHorizontalDoor
                                           : DrawWorldOffsetYVerticalDoor);
            renderer.DrawOrderOffsetY = isHorizontalDoor
                                            ? WallPatterns.DrawOffsetNormal - DrawWorldOffsetYHorizontalDoor
                                            : this.DoorSizeTiles + 0.1 - DrawWorldOffsetYVerticalDoor;

            var spriteSheetAnimator = sceneObject.AddComponent<ClientComponentDoorSpriteSheetAnimator>();
            var atlasColumnsCount = atlas.AtlasSize.ColumnsCount;

            clientState.Renderer?.Destroy();
            clientState.Renderer = renderer;
            clientState.SpriteAnimator?.Destroy();
            clientState.SpriteAnimator = spriteSheetAnimator;

            clientState.DoorBaseRenderer?.Destroy();
            clientState.DoorBaseRenderer = null;

            clientState.DoorVerticalFrontPartRenderer?.Destroy();
            clientState.DoorVerticalFrontPartRenderer = null;

            if (isHorizontalDoor)
            {
                // add extra sprite renderer for horizontal door - door base
                clientState.DoorBaseRenderer = Client.Rendering.CreateSpriteRenderer(
                    sceneObject,
                    this.TextureBaseHorizontal,
                    drawOrder: DrawOrder.Floor + 1,
                    positionOffset: (0, DrawWorldOffsetYHorizontalDoor),
                    spritePivotPoint: (0, 0));
            }
            else
            {
                // add extra sprite renderer for vertical door - door base
                clientState.DoorBaseRenderer = Client.Rendering.CreateSpriteRenderer(
                    sceneObject,
                    atlas.Chunk((byte)(atlasColumnsCount - 1), 0),
                    drawOrder: DrawOrder.Floor + 1,
                    positionOffset: (0, DrawWorldOffsetYVerticalDoor));

                // add extra sprite renderer for vertical door - side door front sprite
                clientState.DoorVerticalFrontPartRenderer = Client.Rendering.CreateSpriteRenderer(
                    sceneObject,
                    atlas.Chunk((byte)(atlasColumnsCount - 2), 0),
                    drawOrder: DrawOrder.Default,
                    positionOffset:
                    (0, DrawWorldOffsetYVerticalDoor));
            }

            var framesCount = isHorizontalDoor
                                  ? atlasColumnsCount
                                  : atlasColumnsCount - 2;
            spriteSheetAnimator.Setup(
                renderer,
                ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                    atlas,
                    columns: (byte)framesCount),
                frameDurationSeconds: this.DoorOpenCloseAnimationDuration / (double)framesCount);

            // refresh opened/closed state
            clientState.IsOpened = isOpened;
            // re-create physics
            this.SharedCreatePhysics(data.GameObject);
            spriteSheetAnimator.SetCurrentFrame(isOpened ? spriteSheetAnimator.FramesTextureResources.Length : 0);
        }

        private bool ServerCanCloseDoor(
            IStaticWorldObject worldObject,
            TPublicState publicState)
        {
            if (!publicState.IsOpened)
            {
                return true;
            }

            // we need to check to ensure there are no obstacles preventing it from closing as it's currently open
            var physicsSpace = worldObject.PhysicsBody.PhysicsSpace;
            ITempList<TestResult> testResult;

            var doorSize = this.DoorSizeTiles;
            if (publicState.IsHorizontalDoor)
            {
                testResult = physicsSpace.TestRectangle(
                    worldObject.TilePosition.ToVector2D() + (0, WallPatterns.PhysicsOffset),
                    size: (doorWidth: doorSize, HorizontalDoorPhysicsHeight),
                    collisionGroup: CollisionGroups.Default);
            }
            else
            {
                testResult = physicsSpace.TestRectangle(
                    worldObject.TilePosition.ToVector2D() + (VerticalDoorPhysicsWidthClosedOffsetX, 0),
                    size: (VerticalDoorPhysicsWidthClosed, doorWidth: doorSize),
                    collisionGroup: CollisionGroups.Default);
            }

            foreach (var result in testResult)
            {
                var protoObject = result.PhysicsBody.AssociatedWorldObject?.ProtoWorldObject;
                if (protoObject is IProtoCharacter
                    || protoObject is IProtoVehicle)
                {
                    // door should be kept opened as there is a character or vehicle
                    // which will stuck if the door is closed
                    return false;
                }
            }

            foreach (var occupiedTile in worldObject.OccupiedTiles)
            foreach (var staticObject in occupiedTile.StaticObjects)
            {
                if (staticObject.ProtoGameObject is ObjectPlayerLootContainer)
                {
                    // don't close, there is a player loot container in the door
                    // (it dropped from the player who died here)
                    return false;
                }
            }

            // no obstacles, can be closed
            return true;
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly List<ICharacter> StaticTempCharactersNearby 
            = new List<ICharacter>(capacity: 64);

        private bool ServerCheckIsDoorShouldBeOpened(
            IStaticWorldObject worldObject,
            TPrivateState privateState)
        {
            var mode = privateState.AccessMode;
            if (mode == WorldObjectAccessMode.Closed)
            {
                return false;
            }

            Server.World.GetInViewScopeByPlayers(worldObject, StaticTempCharactersNearby);
            if (StaticTempCharactersNearby.Count == 0)
            {
                // no characters nearby
                return false;
            }

            var objectOpeningBounds = this.SharedGetDoorOpeningBounds(worldObject);
            foreach (var character in StaticTempCharactersNearby)
            {
                if (!character.ServerIsOnline
                    || character.ProtoCharacter is PlayerCharacterSpectator)
                {
                    continue;
                }

                if (!objectOpeningBounds.Contains(character.Position))
                {
                    // too far from this door
                    continue;
                }

                if (!WorldObjectAccessModeSystem.ServerHasAccess(worldObject,
                                                                 character,
                                                                 mode,
                                                                 writeToLog: false))
                {
                    continue;
                }

                // we don't do this check because it requires character to be the door owner
                //if (!this.SharedCanInteract(character, gameObject, writeToLog: false))
                //{
                //    return false;
                //}

                // we do this check instead:
                // ensure the character is alive and there is a direct line of sight between the character and the door
                var characterPublicState = character.GetPublicState<ICharacterPublicState>();
                if (characterPublicState.IsDead)
                {
                    // dead
                    continue;
                }

                if (!this.IsHeavyVehicleCanPass
                    && characterPublicState is PlayerCharacterPublicState playerCharacterPublicState
                    && playerCharacterPublicState.CurrentVehicle?.ProtoGameObject is IProtoVehicle protoVehicle
                    && protoVehicle.IsHeavyVehicle)
                {
                    // in a heavy vehicle and cannot pass
                    continue;
                }

                var characterPhysicsBody = character.PhysicsBody;
                var characterCenter = character.Position + characterPhysicsBody.CenterOffset;
                if (!SharedHasObstaclesOnTheWay(character,
                                                characterCenter,
                                                characterPhysicsBody.PhysicsSpace,
                                                worldObject,
                                                sendDebugEvents: true))
                {
                    return true;
                }
            }

            // the door should be closed
            return false;
        }
    }

    public abstract class ProtoObjectDoor
        : ProtoObjectDoor<ObjectDoorPrivateState, ObjectDoorPublicState, ObjectDoorClientState>
    {
    }
}