namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
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
              StaticObjectClientState>
    {
        public override bool IsAutoSelectRecipe => true;

        public override bool IsFuelProduceByproducts => false;

        public abstract float LiquidCapacity { get; }

        public abstract float LiquidProductionAmountPerSecond { get; }

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

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var objectDeposit = this.SharedGetDepositWorldObject(data.GameObject.OccupiedTile);
            // force reinitialize deposit to ensure the deposit healthbar will be hidden
            objectDeposit?.ClientInitialize();
        }

        protected override void ClientDeinitializeStructure(IStaticWorldObject gameObject)
        {
            var objectDeposit = this.SharedGetDepositWorldObject(gameObject.OccupiedTile);
            // force reinitialize deposit to ensure the deposit healthbar will be not hidden
            objectDeposit?.ClientInitialize();

            base.ClientDeinitializeStructure(gameObject);
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

        protected override void ClientOnObjectDestroyed(Vector2Ushort tilePosition)
        {
            base.ClientOnObjectDestroyed(tilePosition);

            var objectDeposit = this.SharedGetDepositWorldObject(
                Client.World.GetTile(tilePosition));
            // force reinitialize deposit to ensure the deposit (if it's still there as in case of infinite deposit) will be rendered
            objectDeposit?.ClientInitialize();
        }

        protected virtual void ClientSetupOilPumpActiveAnimation(
            IStaticWorldObject worldObject,
            ObjectManufacturerPublicState serverPublicState,
            ITextureAtlasResource textureAtlasResource,
            Vector2D positionOffset,
            double frameDurationSeconds,
            bool autoInverseAnimation = false,
            bool playSounds = true,
            OnRefreshActiveState onRefresh = null)
        {
            var clientState = worldObject.GetClientState<StaticObjectClientState>();
            var sceneObject = Client.Scene.GetSceneObject(worldObject);

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
                frameDurationSeconds: frameDurationSeconds);

            // we play Active sound for pumping on both up and down position of the oil pump
            var componentActiveState = sceneObject.AddComponent<ClientComponentOilPumpActiveState>();
            componentActiveState.Setup(overlayRenderer, spriteSheetAnimator, worldObject, playSounds);

            serverPublicState.ClientSubscribe(
                s => s.IsManufacturingActive,
                callback: RefreshActiveState,
                subscriptionOwner: clientState);

            spriteSheetAnimator.IsEnabled = overlayRenderer.IsEnabled = false;

            RefreshActiveState(serverPublicState.IsManufacturingActive);

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

            if (weaponCache != null
                && (weaponCache.ProtoWeapon != null
                    || weaponCache.ProtoObjectExplosive != null))
            {
                // the damage was dealt by a weapon or explosive - try to explode the deposit
                var worldObjectDeposit = this.SharedGetDepositWorldObject(
                    Server.World.GetTile(targetObject.TilePosition));
                ((IProtoObjectDeposit)worldObjectDeposit?.ProtoStaticWorldObject)?
                    .ServerOnExtractorDestroyedForDeposit(worldObjectDeposit);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var worldObject = data.GameObject;
            var privateState = data.PrivateState;
            var objectDeposit = this.SharedGetDepositWorldObject(worldObject.OccupiedTile);

            if (objectDeposit == null)
            {
                // no deposit object - stop progressing
                privateState.LiquidContainerState.Amount = 0;
                privateState.FuelBurningState.FuelUseTimeRemainsSeconds = 0;
                return;
            }

            var fuelBurningState = privateState.FuelBurningState;

            // Please note: fuel is used only to produce oil.
            // Fuel is not used for "petroleum canister" crafting.
            var isFull = privateState.LiquidContainerState.Amount >= this.LiquidContainerConfig.Capacity;
            FuelBurningMechanic.Update(
                worldObject,
                fuelBurningState,
                null,
                this.ManufacturingConfig,
                data.DeltaTime,
                isNeedFuelNow: !isFull);

            var isFuelBurning = fuelBurningState.FuelUseTimeRemainsSeconds > 0;
            data.PublicState.IsManufacturingActive = isFuelBurning;

            LiquidContainerSystem.UpdateWithManufacturing(
                worldObject,
                data.PrivateState.LiquidContainerState,
                this.LiquidContainerConfig,
                data.PrivateState.ManufacturingState,
                this.ManufacturingConfig,
                data.DeltaTime,
                // the pump produce petroleum only when fuel is burning
                isProduceLiquid: isFuelBurning,
                forceUpdateRecipe: true);
        }

        protected abstract IStaticWorldObject SharedGetDepositWorldObject(Tile tile);
    }
}