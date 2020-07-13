namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
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

    public abstract class ProtoObjectLandClaim
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectLandClaim,
          IInteractableProtoWorldObject
        where TPrivateState : ObjectLandClaimPrivateState, new()
        where TPublicState : ObjectLandClaimPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public const string DialogCannotUpgrade = "Cannot upgrade";

        public const string NotificationDontHaveAccess = "You don't have access to this land claim.";

        public const string NotificationUpgraded_Message = "Land claim structure has been upgraded.";

        public const string NotificationUpgraded_Title = "Successfully upgraded";

        private readonly Lazy<ushort> lazyLandClaimGraceAreaPaddingSizeOneDirection;

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

        public override bool HasIncreasedScopeSize => true;

        public override string InteractionTooltipText => InteractionTooltipTexts.Configure;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public virtual bool IsShortRaidblockOnHit => false;

        public ushort LandClaimGraceAreaPaddingSizeOneDirection
            => this.lazyLandClaimGraceAreaPaddingSizeOneDirection.Value;

        public abstract ushort LandClaimSize { get; }

        public abstract byte LandClaimTier { get; }

        public ushort LandClaimWithGraceAreaSize
            => (ushort)(this.LandClaimSize
                        + 2 * this.LandClaimGraceAreaPaddingSizeOneDirection);

        public abstract double ShieldProtectionDuration { get; }

        public abstract double ShieldProtectionTotalElectricityCost { get; }

        public virtual ITextureResource TextureResourceObjectBroken { get; protected set; }

        public BaseUserControlWithWindow ClientOpenUI(IWorldObject worldObject)
        {
            return this.ClientOpenUI(new ClientObjectData((IStaticWorldObject)worldObject));
        }

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            base.ClientSetupBlueprint(tile, blueprint);

            if (ConstructionPlacementSystem.IsInObjectPlacementMode)
            {
                ProtoObjectLandClaimPlacementDisplayHelper.SetupBlueprint(tile, blueprint, this);
            }
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

            DialogWindow.ShowDialog(DialogCannotUpgrade,
                                    text: checkResult.GetDescription(),
                                    textAlignment: TextAlignment.Left,
                                    closeByEscapeKey: true);
        }

        public override void ServerApplyDecay(IStaticWorldObject worldObject, double deltaTime)
        {
            var publicState = GetPublicState(worldObject);
            if (publicState.StructurePointsCurrent <= 0)
            {
                // already awaiting destruction
                return;
            }

            base.ServerApplyDecay(worldObject, deltaTime);

            if (publicState.StructurePointsCurrent <= 0)
            {
                // decayed to the point of starting the destroy timer
                var areaPrivateState = LandClaimArea.GetPrivateState(publicState.LandClaimAreaObject);
                areaPrivateState.IsDestroyedByPlayers = false;
            }
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

        public override void ServerOnRepairStageFinished(IStaticWorldObject worldObject, ICharacter character)
        {
            var publicState = GetPublicState(worldObject);
            var areaPrivateState = LandClaimArea.GetPrivateState(publicState.LandClaimAreaObject);
            areaPrivateState.IsDestroyedByPlayers = false;
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

            // notify client (to play a sound)
            ConstructionPlacementSystem.Instance.ServerNotifyOnStructurePlacedOrRelocated(
                upgradedWorldObjectLandClaim,
                character);
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

            if (PlayerCharacterSpectator.SharedIsSpectator(character)
                || CreativeModeSystem.SharedIsInCreativeMode(character))
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
                if (character.Name != founderName
                    && !CreativeModeSystem.SharedIsInCreativeMode(character))
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

            if (result == ObjectLandClaimCanUpgradeCheckResult.Success
                && LandClaimShieldProtectionSystem.SharedIsAreaUnderShieldProtection(currentLandClaimArea))
            {
                // the building is in an area under shield protection
                result = ObjectLandClaimCanUpgradeCheckResult.ErrorUnderShieldProtection;
            }

            if (result == ObjectLandClaimCanUpgradeCheckResult.Success)
            {
                // check there will be no intersection with other areas
                var landClaimCenterTilePosition =
                    LandClaimSystem.SharedCalculateLandClaimObjectCenterTilePosition(worldObjectLandClaim);

                if (!LandClaimSystem.SharedCheckCanPlaceOrUpgradeLandClaimThereConsideringShieldProtection(
                        protoUpgradedLandClaim,
                        landClaimCenterTilePosition,
                        character))
                {
                    result = ObjectLandClaimCanUpgradeCheckResult.ErrorAreaIntersectionWithShieldProtectedArea;
                }

                if (!LandClaimSystem.SharedCheckCanPlaceOrUpgradeLandClaimThere(
                        protoUpgradedLandClaim,
                        landClaimCenterTilePosition,
                        character))
                {
                    result = ObjectLandClaimCanUpgradeCheckResult.ErrorAreaIntersection;
                }

                if (!LandClaimSystem.SharedCheckNoLandClaimByDemoPlayers(
                        protoUpgradedLandClaim,
                        landClaimCenterTilePosition,
                        character,
                        exceptAreasGroup: LandClaimSystem.SharedGetLandClaimAreasGroup(currentLandClaimArea)))
                {
                    result = ObjectLandClaimCanUpgradeCheckResult.ErrorAreaIntersectionDemoPlayer;
                }

                if (IsServer
                    && !LandClaimSystem.ServerCheckFutureBaseWillExceedSafeStorageCapacity(
                        protoUpgradedLandClaim,
                        landClaimCenterTilePosition,
                        character))
                {
                    result = ObjectLandClaimCanUpgradeCheckResult.ErrorExceededSafeStorageCapacity;
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

            if (result == ObjectLandClaimCanUpgradeCheckResult.Success
                && LandClaimSystem.SharedIsUnderRaidBlock(character, worldObjectLandClaim))
            {
                // the building is in an area under raid
                LandClaimSystem.SharedSendNotificationActionForbiddenUnderRaidblock(character);
                result = ObjectLandClaimCanUpgradeCheckResult.ErrorUnderRaid;
            }

            if (writeErrors
                && result != ObjectLandClaimCanUpgradeCheckResult.Success)
            {
                Logger.Warning(
                    $"Can't upgrade: {worldObjectLandClaim} to {protoUpgradedLandClaim}: error code - {result}",
                    character);
            }

            return result;
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            if (IsServer)
            {
                LandClaimSystem.ServerOnRaid(targetObject.Bounds,
                                             weaponCache.Character,
                                             isShort: this.IsShortRaidblockOnHit);
            }

            var publicState = GetPublicState(targetObject);
            var previousStructurePoints = publicState.StructurePointsCurrent;
            if (previousStructurePoints <= 0f)
            {
                // already destroyed land claim object (waiting for the destroy timer)
                obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
                damageApplied = 0;

                if (IsServer
                    && weaponCache.Character != null)
                {
                    var areaPrivateState = LandClaimArea.GetPrivateState(publicState.LandClaimAreaObject);
                    areaPrivateState.IsDestroyedByPlayers = true;
                }

                return true;
            }

            return base.SharedOnDamage(weaponCache,
                                       targetObject,
                                       damagePreMultiplier,
                                       out obstacleBlockDamageCoef,
                                       out damageApplied);
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
            var area = LandClaimSystem.ServerGetLandClaimArea((IStaticWorldObject)worldObject);
            if (area == null)
            {
                // area could be null in the Editor for the land claim without owners
                return;
            }

            var areasGroup = LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
            if (!LandClaimSystem.ServerIsOwnedArea(area, who)
                && (PlayerCharacterSpectator.SharedIsSpectator(who)
                    || CreativeModeSystem.SharedIsInCreativeMode(who)))
            {
                Server.World.EnterPrivateScope(who, area);
            }

            Server.World.EnterPrivateScope(who, areasGroup);
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
            var area = LandClaimSystem.ServerGetLandClaimArea((IStaticWorldObject)worldObject);
            if (area == null)
            {
                // area could be null in the Editor for the land claim without owners
                return;
            }

            var areasGroup = LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
            if (!LandClaimSystem.ServerIsOwnedArea(area, who)
                && (PlayerCharacterSpectator.SharedIsSpectator(who)
                    || CreativeModeSystem.SharedIsInCreativeMode(who)))
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
                worldObject.ClientSceneObject);

            // subscribe to destruction timer
            publicState.ClientSubscribe(
                _ => _.ServerTimeForDestruction,
                _ => ClientRefreshSprite(),
                clientState);

            ClientRefreshSprite();

            Client.UI.AttachControl(
                worldObject,
                new BrokenObjectLandClaimTooltip()
                {
                    ObjectLandClaim = worldObject,
                    ObjectLandClaimPublicState = data.PublicState,
                    VerticalAlignment = VerticalAlignment.Bottom
                },
                positionOffset: this.SharedGetObjectCenterWorldOffset(worldObject)
                                + (0, 0.46),
                isFocusable: false);

            void ClientRefreshSprite()
            {
                clientState.Renderer.TextureResource = publicState.ServerTimeForDestruction.HasValue
                                                           ? this.TextureResourceObjectBroken
                                                           : this.DefaultTexture;
            }
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
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
            tileRequirements
                .Clear()
                .Add(ConstructionTileRequirements.DefaultForPlayerStructuresOwnedOrFreeLand)
                // land-claim specific requirements
                .Add(LandClaimSystem.ValidatorNewLandClaimNoLandClaimIntersectionsWithShieldProtection)
                .Add(LandClaimSystem.ValidatorNewLandClaimNoLandClaimIntersections)
                .Add(LandClaimSystem.ValidatorCheckCharacterLandClaimAmountLimit)
                .Add(LandClaimSystem.ValidatorNewLandClaimNoLandClaimIntersectionsWithDemoPlayers)
                .Add(LandClaimSystem.ValidatorNewLandClaimNoLandClaimsTooClose)
                .Add(LandClaimSystem.ValidatorCheckLandClaimDepositRequireXenogeology)
                .Add(LandClaimSystem.ValidatorCheckLandClaimDepositCooldown)
                .Add(LandClaimSystem.ValidatorCheckLandClaimBaseSizeLimitNotExceeded)
                .Add(LandClaimSystem.ValidatorNewLandClaimSafeStorageCapacityNotExceeded)
                .Add(ObjectMineralPragmiumSource.ValidatorCheckNoPragmiumSourceNearbyOnPvE);

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

            if (byCharacter != null)
            {
                var areaPrivateState = LandClaimArea.GetPrivateState(publicState.LandClaimAreaObject);
                areaPrivateState.IsDestroyedByPlayers = true;
            }

            if (publicState.ServerTimeForDestruction.HasValue)
            {
                // destruction timer is already set
                return;
            }

            // the land claim structure points is zero - it's broken now - set timer for destruction
            var timeout = PveSystem.ServerIsPvE
                              ? 0
                              : this.DestructionTimeout.TotalSeconds;
            publicState.ServerTimeForDestruction = Server.Game.FrameTime + timeout;

            Logger.Important($"Timer for destruction set: {targetObject}. Timeout: {timeout}");
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
                .AddShapeRectangle((width, 0.55),
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