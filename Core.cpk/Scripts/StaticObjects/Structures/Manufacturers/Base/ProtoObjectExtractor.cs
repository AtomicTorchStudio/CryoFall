namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;
    using static ObjectExtractorConstants;

    public abstract class ProtoObjectExtractor
        : ProtoObjectManufacturer<
              ObjectExtractorPrivateState,
              ObjectManufacturerPublicState,
              StaticObjectClientState>,
          IProtoObjectExtractor
    {
        public const string Error_CannotBuildTooCloseToDeposit
            = "Too close to a resource deposit. You can build right over it.";

        public const string Error_TooCloseToDepletedDeposit
            = "Too close to a depleted deposit.";

        public const double FuelBurningSpeedMultiplier = 0.5;

        private const int MinDistanceBetweenExtractors = 11;

        protected static readonly ConstructionTileRequirements.Validator ValidatorTooCloseToDepletedDeposit
            = new ConstructionTileRequirements.Validator(
                Error_TooCloseToDepletedDeposit,
                c =>
                {
                    if (c.TileOffset != default)
                    {
                        return true;
                    }

                    var startPosition = c.Tile.Position;
                    var objectsInBounds = SharedFindObjectsNearby<ObjectDepletedDeposit>(startPosition);
                    if (objectsInBounds.Any())
                    {
                        // found a depleted deposit nearby
                        return false;
                    }

                    return true;
                });

        public override ProtoObjectConstructionSite ConstructionSitePrototype
            => Api.GetProtoEntity<ObjectConstructionSiteForExtractor>();

        public abstract byte ContainerInputSlotsCountExtractorWithDeposit { get; }

        public abstract byte ContainerOutputSlotsCountExtractorWithDeposit { get; }

        // necessary for distance checks
        public override bool HasIncreasedScopeSize => true;

        public override bool IsAutoSelectRecipe => true;

        public override bool IsFuelProduceByproducts => false;

        public abstract double LiquidCapacity { get; }

        public abstract double LiquidProductionAmountPerSecond { get; }

        protected LiquidContainerConfig LiquidContainerConfig { get; private set; }

        public override void ServerApplyDecay(IStaticWorldObject worldObject, double deltaTime)
        {
            var restrictedZone = ZoneSpecialConstructionRestricted.Instance
                                                                  .ServerZoneInstance;
            if (restrictedZone.IsContainsPosition(
                worldObject.OccupiedTile.NeighborTileDownLeft.Position))
            {
                // this is a public Oil pump or Lithium extractor
                // should not decay
                return;
            }

            base.ServerApplyDecay(worldObject, deltaTime);
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            var deposit = this.SharedGetDepositWorldObject(worldObject.OccupiedTile);
            if (deposit is null
                || (deposit.ProtoStaticWorldObject is IProtoObjectDeposit protoDeposit
                    && protoDeposit.LifetimeTotalDurationSeconds > 0))
            {
                // check whether player can interact with this extractor (such as PvE checks for ownership, etc)
                return base.SharedCanInteract(character, worldObject, writeToLog);
            }

            // don't perform extra checks as this is an extractor for a public (infinite) deposit
            return this.SharedIsInsideCharacterInteractionArea(character,
                                                               worldObject,
                                                               writeToLog);
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            foreach (var tileObject in targetObject.OccupiedTile.StaticObjects)
            {
                if (tileObject.ProtoStaticWorldObject is IProtoObjectDeposit protoObjectDeposit
                    && protoObjectDeposit.LifetimeTotalDurationSeconds <= 0)
                {
                    // damaging extractor built over the infinite deposit
                    damageApplied = 0;
                    obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
                    return true;
                }
            }

            return base.SharedOnDamage(weaponCache,
                                       targetObject,
                                       damagePreMultiplier,
                                       out obstacleBlockDamageCoef,
                                       out damageApplied);
        }

        // Please note: the start position is located in bottom left corner of the layout.
        protected static IEnumerable<IStaticWorldObject> SharedFindObjectsNearby
            <TProtoObject>(Vector2Ushort startPosition)
            where TProtoObject : class, IProtoStaticWorldObject
        {
            var world = IsServer
                            ? (IWorldService)Server.World
                            : (IWorldService)Client.World;

            // TODO: the offset here should be taken from current prototype Layout (its center)
            var bounds = new RectangleInt(startPosition + (1, 1),
                                          size: (1, 1));
            bounds = bounds.Inflate(MinDistanceBetweenExtractors);

            var objectsInBounds = world.GetStaticWorldObjectsOfProtoInBounds<TProtoObject>(bounds);
            return objectsInBounds;
        }

        protected override void ClientDeinitializeStructure(IStaticWorldObject gameObject)
        {
            var objectDeposit = this.SharedGetDepositWorldObject(gameObject.OccupiedTile);
            // force reinitialize deposit to ensure the deposit healthbar will be not hidden
            objectDeposit?.ClientInitialize();

            base.ClientDeinitializeStructure(gameObject);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var objectDeposit = this.SharedGetDepositWorldObject(data.GameObject.OccupiedTile);
            // force reinitialize deposit to ensure the deposit healthbar will be hidden
            objectDeposit?.ClientInitialize();

            if (objectDeposit != null
                && ((IProtoObjectDeposit)objectDeposit.ProtoGameObject).LifetimeTotalDurationSeconds <= 0)
            {
                // this extractor is built over an infinite source
                // remove a "broken shield" icon as this area cannot be claimed
                // this extractor cannot be damaged anyway (see SharedOnDamage method)
                StructureLandClaimIndicatorManager.ClientDeinitialize(data.GameObject);
            }
        }

        protected override void ClientObserving(ClientObjectData data, bool isObserving)
        {
            base.ClientObserving(data, isObserving);

            var worldObjectDeposit = this.SharedGetDepositWorldObject(data.GameObject.OccupiedTile);

            if (isObserving)
            {
                // display tooltip only if we have a deposit here
                isObserving = worldObjectDeposit != null;
            }

            ClientObjectDepositTooltipHelper.Refresh(worldObjectDeposit, isObserving);
        }

        protected override void ClientOnObjectDestroyed(Vector2D position)
        {
            base.ClientOnObjectDestroyed(position);

            var objectDeposit = this.SharedGetDepositWorldObject(
                Client.World.GetTile(position.ToVector2Ushort()));
            // force reinitialize deposit to ensure the deposit (if it's still there as in case of infinite deposit) will be rendered
            objectDeposit?.ClientInitialize();
        }

        protected virtual void ClientSetupExtractorActiveAnimation(
            IStaticWorldObject worldObject,
            ObjectManufacturerPublicState serverPublicState,
            ITextureAtlasResource textureAtlasResource,
            Vector2D positionOffset,
            double frameDurationSeconds,
            bool autoInverseAnimation = false,
            bool randomizeInitialFrame = false,
            bool playAnimationSounds = true,
            OnRefreshActiveState onRefresh = null)
        {
            var clientState = worldObject.GetClientState<StaticObjectClientState>();
            var sceneObject = worldObject.ClientSceneObject;

            var overlayRenderer = Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                TextureResource.NoTexture,
                DrawOrder.Default,
                positionOffset: positionOffset,
                spritePivotPoint: Vector2D.Zero);
            overlayRenderer.DrawOrderOffsetY = -positionOffset.Y - 0.01;

            var spriteSheetAnimator = sceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
            spriteSheetAnimator.Setup(
                overlayRenderer,
                ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                    textureAtlasResource,
                    autoInverse: autoInverseAnimation),
                isLooped: true,
                frameDurationSeconds: frameDurationSeconds,
                randomizeInitialFrame: randomizeInitialFrame);

            // we play Active sound for pumping on both up and down position of the oil pump
            var componentActiveState = sceneObject.AddComponent<ClientComponentOilPumpActiveState>();
            componentActiveState.Setup(overlayRenderer, spriteSheetAnimator, worldObject, playAnimationSounds);

            serverPublicState.ClientSubscribe(
                s => s.IsActive,
                callback: RefreshActiveState,
                subscriptionOwner: clientState);

            spriteSheetAnimator.IsEnabled = overlayRenderer.IsEnabled = false;

            RefreshActiveState(serverPublicState.IsActive);

            void RefreshActiveState(bool isActive)
            {
                // ReSharper disable once PossibleNullReferenceException
                componentActiveState.IsActive = isActive;
                onRefresh?.Invoke(isActive);
            }
        }

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.LiquidContainerConfig = new LiquidContainerConfig(
                capacity: this.LiquidCapacity,
                amountAutoIncreasePerSecond: this.LiquidProductionAmountPerSecond,
                // petroleum decrease happens automatically when crafting of the "canister with petroleum" recipe finishes
                amountAutoDecreasePerSecondWhenUse: 0);
        }

        protected override byte ServerGetInputSlotsCount(IStaticWorldObject worldObject)
        {
            var deposit = this.SharedGetDepositWorldObject(worldObject.OccupiedTile);
            return deposit is null
                       ? this.ContainerInputSlotsCount
                       : this.ContainerInputSlotsCountExtractorWithDeposit;
        }

        protected override byte ServerGetOutputSlotsCount(IStaticWorldObject worldObject)
        {
            var deposit = this.SharedGetDepositWorldObject(worldObject.OccupiedTile);
            return deposit is null
                       ? this.ContainerOutputSlotsCount
                       : this.ContainerOutputSlotsCountExtractorWithDeposit;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            var statePrivate = data.PrivateState;

            // setup input container to allow only empty canisters on input
            Server.Items.SetContainerType<ItemsContainerEmptyCanisters>(
                statePrivate.ManufacturingState.ContainerInput);

            if (statePrivate.LiquidContainerState == null)
            {
                statePrivate.LiquidContainerState = new LiquidContainerState();
            }
        }

        protected override void ServerOnStaticObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            base.ServerOnStaticObjectZeroStructurePoints(weaponCache, byCharacter, targetObject);

            if (weaponCache == null
                || (weaponCache.ProtoWeapon == null
                    && weaponCache.ProtoExplosive == null)
                || PveSystem.ServerIsPvE)
            {
                return;
            }

            // explode
            var tilePosition = targetObject.TilePosition;
            Server.World.CreateStaticWorldObject<ObjectDepositExplosion>(tilePosition);

            // the damage was dealt by a weapon or explosive - try to explode the deposit
            var worldObjectDeposit = this.SharedGetDepositWorldObject(
                Server.World.GetTile(tilePosition));

            if (worldObjectDeposit != null)
            {
                ((IProtoObjectDeposit)worldObjectDeposit.ProtoStaticWorldObject)
                    .ServerOnExtractorDestroyedForDeposit(worldObjectDeposit);
            }
            else
            {
                // create charred ground at the center of the explosion
                Server.World.CreateStaticWorldObject(GetProtoEntity<ObjectCharredGround1>(),
                                                     (tilePosition + this.Layout.Center.ToVector2Int())
                                                     .ToVector2Ushort());
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var worldObject = data.GameObject;
            var privateState = data.PrivateState;
            var objectDepletedDeposit =
                ObjectDepletedDeposit.SharedGetDepletedDepositWorldObject(worldObject.OccupiedTile);
            var objectDeposit = this.SharedGetDepositWorldObject(worldObject.OccupiedTile);

            var fuelBurningState = privateState.FuelBurningState;
            if (objectDepletedDeposit != null)
            {
                // this is a depleted deposit - stop extraction
                data.PublicState.IsActive = false;
                privateState.LiquidContainerState.Amount = 0;
                if (fuelBurningState != null)
                {
                    fuelBurningState.FuelUseTimeRemainsSeconds = 0;
                }

                return;
            }

            var isActive = false;
            var isFull = privateState.LiquidContainerState.Amount >= this.LiquidContainerConfig.Capacity;

            if (fuelBurningState == null)
            {
                // no fuel burning state
                if (this.ElectricityConsumptionPerSecondWhenActive <= 0)
                {
                    // no fuel burning and no electricity consumption - always active
                    isActive = true;
                }
                else
                {
                    // Consuming electricity.
                    // Active only if electricity state is on and has active recipe.
                    var publicState = data.PublicState;
                    if (publicState.ElectricityConsumerState == ElectricityConsumerState.PowerOn)
                    {
                        isActive = !isFull;
                    }
                }
            }
            else
            {
                // Please note: fuel is used only to produce oil.
                // Fuel is not used for "petroleum canister" crafting.
                FuelBurningMechanic.Update(
                    worldObject,
                    fuelBurningState,
                    byproductsCraftQueue: null,
                    this.ManufacturingConfig,
                    data.DeltaTime * FuelBurningSpeedMultiplier,
                    byproductsQueueRate: 1,
                    isNeedFuelNow: !isFull);

                var isFuelBurning = fuelBurningState.FuelUseTimeRemainsSeconds > 0;
                isActive = isFuelBurning;
            }

            data.PublicState.IsActive = isActive;

            LiquidContainerSystem.UpdateWithManufacturing(
                worldObject,
                data.PrivateState.LiquidContainerState,
                this.LiquidContainerConfig,
                data.PrivateState.ManufacturingState,
                this.ManufacturingConfig,
                data.DeltaTime * SharedGetSpeedMultiplier(objectDeposit),
                // the pump produce petroleum only when active
                isProduceLiquid: data.PublicState.IsActive,
                forceUpdateRecipe: true);
        }

        protected abstract IStaticWorldObject SharedGetDepositWorldObject(Tile tile);

        private static double SharedGetSpeedMultiplier(IStaticWorldObject objectDeposit)
        {
            return GetDepositExtractionRateMultiplier()
                   * StructureConstants.DepositsExtractionSpeedMultiplier;

            double GetDepositExtractionRateMultiplier()
            {
                if (PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                {
                    return objectDeposit is null
                               ? ExtractorPvE
                               : InfiniteExtractorPvE;
                }

                // extractor in PvP
                if (objectDeposit is null)
                {
                    return ExtractorPvpWithoutDeposit;
                }

                var protoObjectDeposit = (IProtoObjectDeposit)objectDeposit.ProtoStaticWorldObject;
                return protoObjectDeposit.LifetimeTotalDurationSeconds <= 0
                           ? InfiniteExtractorPvP
                           : ExtractorPvpWithDeposit;
            }
        }
    }
}