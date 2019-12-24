namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectExtractor
        : ProtoObjectManufacturer<
              ObjectExtractorPrivateState,
              ObjectManufacturerPublicState,
              StaticObjectClientState>,
          IProtoObjectExtractor
    {
        /// <summary>
        /// These constants are adjusting both extraction speed and fuel consumption speed.
        /// </summary>
        public const double PveInfiniteExtractorSpeedMultiplier = 0.5;

        public const double PvePlayerBuiltExtractorSpeedMultiplier = 0.25;

        public const double PvpInfiniteExtractorSpeedMultiplier = 1.0;

        public const double PvpPlayerBuiltExtractorSpeedMultiplier = 0.5;

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

        public override double SharedGetCurrentElectricityConsumptionRate(IStaticWorldObject worldObject)
        {
            var rate = base.SharedGetCurrentElectricityConsumptionRate(worldObject);
            if (rate <= 0)
            {
                return 0;
            }

            var objectDeposit = this.SharedGetDepositWorldObject(worldObject.OccupiedTile);
            rate *= SharedGetSpeedMultiplier(objectDeposit);
            return rate;
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
                    && weaponCache.ProtoObjectExplosive == null)
                || PveSystem.ServerIsPvE)
            {
                return;
            }

            // the damage was dealt by a weapon or explosive - try to explode the deposit
            var worldObjectDeposit = this.SharedGetDepositWorldObject(
                Server.World.GetTile(targetObject.TilePosition));
            ((IProtoObjectDeposit)worldObjectDeposit?.ProtoStaticWorldObject)?
                .ServerOnExtractorDestroyedForDeposit(worldObjectDeposit);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var worldObject = data.GameObject;
            var privateState = data.PrivateState;
            var objectDeposit = this.SharedGetDepositWorldObject(worldObject.OccupiedTile);

            var fuelBurningState = privateState.FuelBurningState;
            if (objectDeposit == null
                && !PveSystem.ServerIsPvE)
            {
                // no deposit object in PvP - stop extraction
                data.PublicState.IsActive = false;
                privateState.LiquidContainerState.Amount = 0;
                if (fuelBurningState != null)
                {
                    fuelBurningState.FuelUseTimeRemainsSeconds = 0;
                }

                return;
            }

            var deltaTime = data.DeltaTime * SharedGetSpeedMultiplier(objectDeposit);

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
                    deltaTime,
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
                deltaTime,
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
                    return objectDeposit is null // is player built deposit?
                               ? PvePlayerBuiltExtractorSpeedMultiplier
                               : PveInfiniteExtractorSpeedMultiplier;
                }

                // extractor in PvP
                if (objectDeposit == null)
                {
                    // should never happen, there is no deposit so no extraction can go on
                    return 0;
                }

                var protoObjectDeposit = (IProtoObjectDeposit)objectDeposit.ProtoStaticWorldObject;
                return protoObjectDeposit.LifetimeTotalDurationSeconds <= 0 // is infinite deposit?
                           ? PvpInfiniteExtractorSpeedMultiplier
                           : PvpPlayerBuiltExtractorSpeedMultiplier;
            }
        }
    }
}