namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
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

        private static readonly RenderingMaterial ClientBlueprintAreaRenderingMaterial;

        private static readonly RenderingMaterial ClientBlueprintGraceAreaRenderingMaterial;

        private static readonly RenderingMaterial ClientBlueprintRestrictedTileRenderingMaterial;

        private readonly Lazy<ushort> lazyLandClaimGraceAreaPaddingSizeOneDirection;

        private IComponentAttachedControl currentDisplayedTooltip;

        static ProtoObjectLandClaim()
        {
            if (IsClient)
            {
                // prepare material and effect parameters
                ClientBlueprintAreaRenderingMaterial = ClientLandClaimGroupRenderer.CreateRenderingMaterial();
                ClientBlueprintGraceAreaRenderingMaterial = ClientLandClaimGroupRenderer.CreateRenderingMaterial();
                ClientBlueprintGraceAreaRenderingMaterial.EffectParameters.Set("Color", BlueprintAreaColorGrace);

                ClientBlueprintRestrictedTileRenderingMaterial = ClientLandClaimGroupRenderer.CreateRenderingMaterial();
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

        public abstract TimeSpan DecayDelayDuration { get; }

        public abstract TimeSpan DestructionTimeout { get; }

        public override string InteractionTooltipText => InteractionTooltipTexts.Configure;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override bool IsRepeatPlacement => false;

        public ushort LandClaimGraceAreaPaddingSizeOneDirection
            => this.lazyLandClaimGraceAreaPaddingSizeOneDirection.Value;

        public abstract ushort LandClaimSize { get; }

        public ushort LandClaimWithGraceAreaSize
            => (ushort)(this.LandClaimSize
                        + 2 * this.LandClaimGraceAreaPaddingSizeOneDirection);

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
                    ClientLandClaimGroupRenderer.TextureResourceLandClaimAreaCell,
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

            // upgrade (it will destroy an existing structure and place new in its place)
            var upgradedWorldObjectLandClaim = LandClaimSystem.ServerUpgrade(oldWorldObjectLandClaim,
                                                                             upgradeStructure,
                                                                             character);

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
                if (!InteractionCheckerSystem.SharedHasInteraction(character,
                                                                   worldObjectLandClaim,
                                                                   requirePrivateScope: true))
                {
                    result = ObjectLandClaimCanUpgradeCheckResult.ErrorNoActiveInteraction;
                }
            }

            if (result == ObjectLandClaimCanUpgradeCheckResult.Success)
            {
                if (LandClaimSystem.SharedIsUnderRaidBlock(character, worldObjectLandClaim))
                {
                    // the building is in an area under the raid
                    LandClaimSystem.SharedSendNotificationActionForbiddenUnderRaidblock(character);
                    result = ObjectLandClaimCanUpgradeCheckResult.ErrorUnderRaid;
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
            var area = LandClaimSystem.ServerGetLandClaimArea(worldObject);
            if (area == null)
            {
                // area could be null in the Editor for the land claim without owners
                return;
            }

            var areasGroup = LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
            if (CreativeModeSystem.SharedIsInCreativeMode(who)
                && !LandClaimSystem.ServerIsOwnedArea(area, who))
            {
                Server.World.EnterPrivateScope(who, area);
            }

            Server.World.EnterPrivateScope(who, areasGroup);
        }

        void IInteractableProtoStaticWorldObject.ServerOnMenuClosed(ICharacter who, IStaticWorldObject worldObject)
        {
            var area = LandClaimSystem.ServerGetLandClaimArea(worldObject);
            if (area == null)
            {
                // area could be null in the Editor for the land claim without owners
                return;
            }

            var areasGroup = LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
            if (CreativeModeSystem.SharedIsInCreativeMode(who)
                && !LandClaimSystem.ServerIsOwnedArea(area, who))
            {
                Server.World.ExitPrivateScope(who, area);
            }

            Server.World.ExitPrivateScope(who, areasGroup);
        }

        protected abstract BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject);

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;
            var publicState = data.PublicState;
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
                ObjectLandClaimPublicState = data.PublicState,
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
                new ViewModelWindowLandClaim(data.GameObject, data.PublicState.LandClaimAreaObject));
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
            tileRequirements.Add(LandClaimSystem.ValidatorNewLandClaimNoLandClaimsNearby);
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

        protected override void ServerOnStaticObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            // do not use default implementation because it will destroy the object automatically
            //base.ServerOnStaticObjectZeroStructurePoints(weaponCache, targetObject);

            var worldObject = (IStaticWorldObject)targetObject;
            var publicState = GetPublicState(worldObject);
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

                    this.ServerForceUpdate(worldObject, publicState);
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
            this.ServerForceUpdate(worldObject, publicState);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var gameObject = data.GameObject;
            var publicState = data.PublicState;
            this.ServerForceUpdate(gameObject, publicState);
        }

        /// <summary>
        /// Do not override the physics definition as otherwise close players
        /// might stuck in this land claim building after the upgrade.
        /// </summary>
        protected sealed override void SharedCreatePhysics(CreatePhysicsData data)
        {
            const double width = 1.15,
                         height = 1,
                         offsetX = (2 - width) / 2,
                         offsetY = 0.6;

            data.PhysicsBody
                .AddShapeRectangle((width, height),
                                   offset: (offsetX, offsetY))
                .AddShapeRectangle((width, 0.45),
                                   offset: (offsetX, 0.6 + offsetY),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((width, 0.25),
                                   offset: (offsetX, 0.85 + offsetY),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((1.2, height + 0.2),
                                   offset: (0.4, offsetY),
                                   group: CollisionGroups.ClickArea);
        }

        private void ClientRemote_OnCannotInteract(IStaticWorldObject worldObject, LandClaimMenuOpenResult result)
        {
            if (result == LandClaimMenuOpenResult.FailPlayerIsNotOwner)
            {
                ClientOnCannotInteract(worldObject,
                                       NotificationDontHaveAccess,
                                       isOutOfRange: false);
                return;
            }

            Logger.Warning($"Received cannot open land menu result: {worldObject},  result={result}");
        }

        private void ServerForceUpdate(IStaticWorldObject gameObject, TPublicState publicState)
        {
            if (!publicState.ServerTimeForDestruction.HasValue)
            {
                return;
            }

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
    }

    public abstract class ProtoObjectLandClaim
        : ProtoObjectLandClaim
            <ObjectLandClaimPrivateState,
                ObjectLandClaimPublicState,
                StaticObjectClientState>
    {
    }
}