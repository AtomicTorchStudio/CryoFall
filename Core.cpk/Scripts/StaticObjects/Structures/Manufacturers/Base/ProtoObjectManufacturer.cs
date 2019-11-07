namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    [PrepareOrder(afterType: typeof(Recipe))]
    public abstract class ProtoObjectManufacturer
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectManufacturer,
          IProtoObjectElectricityConsumerWithCustomRate,
          IInteractableProtoWorldObject
        where TPrivateState : ObjectManufacturerPrivateState, new()
        where TPublicState : ObjectManufacturerPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        protected delegate void OnRefreshActiveState(bool isActive);

        public abstract byte ContainerFuelSlotsCount { get; }

        public abstract byte ContainerInputSlotsCount { get; }

        public abstract byte ContainerOutputSlotsCount { get; }

        public virtual double ElectricityConsumptionPerSecondWhenActive => 0;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public abstract bool IsAutoSelectRecipe { get; }

        public abstract bool IsFuelProduceByproducts { get; }

        public ManufacturingConfig ManufacturingConfig { get; private set; }

        public virtual double ManufacturingSpeedMultiplier => 1.0;

        public override double ServerUpdateIntervalSeconds => 0.2;

        public void ClientSelectRecipe(IStaticWorldObject worldObject, Recipe recipe)
        {
            if (!recipe.SharedIsTechUnlocked(Client.Characters.CurrentPlayerCharacter))
            {
                Logger.Error("Cannot select locked recipe: " + recipe);
                return;
            }

            this.CallServer(_ => _.ServerRemote_SelectRecipe(worldObject, recipe));
        }

        public virtual double SharedGetCurrentElectricityConsumptionRate(IStaticWorldObject worldObject)
        {
            return GetPublicState(worldObject).IsActive
                       ? 1
                       : 0;
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            return this.ClientOpenUI(new ClientObjectData((IStaticWorldObject)worldObject));
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        protected virtual IComponentSoundEmitter ClientCreateActiveStateSoundEmitterComponent(
            IStaticWorldObject worldObject)
        {
            return Client.Audio.CreateSoundEmitter(
                worldObject,
                soundResource: worldObject.ProtoStaticWorldObject
                                          .SharedGetObjectSoundPreset()
                                          .GetSound(ObjectSound.Active),
                is3D: true,
                radius: Client.Audio.CalculateObjectSoundRadius(worldObject),
                isLooped: true);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            PowerGridSystem.ClientInitializeConsumerOrProducer(data.GameObject);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected virtual BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowManufacturer.Open(
                new ViewModelWindowManufacturer(
                    data.GameObject,
                    data.PrivateState,
                    this.ManufacturingConfig));
        }

        protected void ClientSetupManufacturerActiveAnimation(
            IStaticWorldObject worldObject,
            ObjectManufacturerPublicState serverPublicState,
            ITextureAtlasResource textureAtlasResource,
            Vector2D positionOffset,
            double frameDurationSeconds,
            double drawOrderOffsetY = 0,
            bool autoInverseAnimation = false,
            bool randomizeInitialFrame = false,
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
            overlayRenderer.DrawOrderOffsetY = drawOrderOffsetY - positionOffset.Y - 0.01;

            var spriteSheetAnimator = sceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
            spriteSheetAnimator.Setup(
                overlayRenderer,
                ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                    textureAtlasResource,
                    autoInverse: autoInverseAnimation),
                frameDurationSeconds: frameDurationSeconds,
                randomizeInitialFrame: randomizeInitialFrame);

            var soundEmitter = this.ClientCreateActiveStateSoundEmitterComponent(worldObject);
            if (clientState.SoundEmitter != null)
            {
                Logger.Error("Sound emitter will be overwritten for: " + worldObject);
            }

            clientState.SoundEmitter = soundEmitter;

            serverPublicState.ClientSubscribe(
                s => s.IsActive,
                callback: RefreshActiveState,
                subscriptionOwner: clientState);

            RefreshActiveState(serverPublicState.IsActive);

            void RefreshActiveState(bool isActive)
            {
                overlayRenderer.IsEnabled = isActive;
                spriteSheetAnimator.IsEnabled = isActive;
                if (soundEmitter != null)
                {
                    soundEmitter.IsEnabled = isActive;
                }

                onRefresh?.Invoke(isActive);
            }
        }

        protected virtual ManufacturingConfig PrepareManufacturingConfig()
        {
            // setup manufacturing recipes
            var recipes = FindProtoEntities<Recipe.RecipeForManufacturing>()
                .Where(r => r.StationTypes.Contains(this));

            IEnumerable<Recipe.RecipeForManufacturingByproduct> recipesForByproducts;

            if (this.IsFuelProduceByproducts
                && this.ContainerFuelSlotsCount > 0)
            {
                recipesForByproducts = FindProtoEntities<Recipe.RecipeForManufacturingByproduct>()
                    .Where(r => r.StationTypes.Count == 0 || r.StationTypes.Contains(this));
            }
            else
            {
                recipesForByproducts = Enumerable.Empty<Recipe.RecipeForManufacturingByproduct>();
            }

            return new ManufacturingConfig(
                this,
                recipes,
                recipesForByproducts,
                this.IsFuelProduceByproducts,
                this.IsAutoSelectRecipe);
        }

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            var manufacturingConfig = this.PrepareManufacturingConfig();
            this.ManufacturingConfig = manufacturingConfig;
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return base.PrepareSoundPresetObject().Clone()
                       .Replace(ObjectSound.Active, "Objects/Structures/" + this.GetType().Name + "/Active");
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            var privateState = data.PrivateState;

            // configure manufacturing state
            var manufacturingState = privateState.ManufacturingState;
            {
                if (manufacturingState == null)
                {
                    manufacturingState = new ManufacturingState(
                        data.GameObject,
                        containerInputSlotsCount: this.ContainerInputSlotsCount,
                        containerOutputSlotsCount: this.ContainerOutputSlotsCount);
                    privateState.ManufacturingState = manufacturingState;
                }
                else
                {
                    manufacturingState.SetSlotsCount(
                        input: this.ContainerInputSlotsCount,
                        output: this.ContainerOutputSlotsCount);
                }

                Server.Items.SetContainerType<ItemsContainerOutput>(
                    manufacturingState.ContainerOutput);
            }

            // configure fuel burning state
            var fuelBurningState = privateState.FuelBurningState;
            {
                if (fuelBurningState == null)
                {
                    if (this.ContainerFuelSlotsCount > 0)
                    {
                        fuelBurningState = new FuelBurningState(data.GameObject, this.ContainerFuelSlotsCount);
                        privateState.FuelBurningState = fuelBurningState;
                    }
                }
                else if (this.ContainerFuelSlotsCount > 0)
                {
                    fuelBurningState.SetSlotsCount(this.ContainerFuelSlotsCount);
                }
                else
                {
                    // destroy fuel burning state
                    privateState.FuelBurningState = fuelBurningState = null;
                }

                if (fuelBurningState != null)
                {
                    Server.Items.SetContainerType<ItemsContainerFuelSolid>(
                        fuelBurningState.ContainerFuel);
                }
            }

            if (this.ManufacturingConfig.IsProduceByproducts)
            {
                if (fuelBurningState == null)
                {
                    throw new Exception(
                        $"No fuel container - please set {nameof(this.ContainerFuelSlotsCount)} higher than zero?");
                }

                if (privateState.FuelBurningByproductsQueue == null)
                {
                    privateState.FuelBurningByproductsQueue = new CraftingQueue(
                        fuelBurningState.ContainerFuel,
                        manufacturingState.ContainerOutput);
                }
            }
            else
            {
                privateState.FuelBurningByproductsQueue = null;
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var privateState = data.PrivateState;
            var manufacturingState = privateState.ManufacturingState;
            var worldObject = data.GameObject;

            // update active recipe
            ManufacturingMechanic.UpdateRecipeOnly(
                worldObject,
                manufacturingState,
                this.ManufacturingConfig);

            var hasActiveRecipe = manufacturingState.HasActiveRecipe;

            var isActive = false;
            var fuelBurningState = privateState.FuelBurningState;

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
                        isActive = hasActiveRecipe
                                   && !manufacturingState.CraftingQueue.IsContainerOutputFull;
                    }
                }
            }
            else
            {
                // progress fuel burning
                FuelBurningMechanic.Update(
                    worldObject,
                    fuelBurningState,
                    privateState.FuelBurningByproductsQueue,
                    this.ManufacturingConfig,
                    deltaTime: data.DeltaTime,
                    byproductsQueueRate: StructureConstants.ManufacturingSpeedMultiplier,
                    isNeedFuelNow: hasActiveRecipe
                                   && !manufacturingState.CraftingQueue.IsContainerOutputFull);

                // active only when fuel is burning
                isActive = fuelBurningState.FuelUseTimeRemainsSeconds > 0;
            }

            data.PublicState.IsActive = isActive;

            if (isActive
                && hasActiveRecipe)
            {
                // progress crafting
                ManufacturingMechanic.UpdateCraftingQueueOnly(
                    manufacturingState,
                    deltaTime: data.DeltaTime
                               * StructureConstants.ManufacturingSpeedMultiplier
                               * this.ManufacturingSpeedMultiplier);

                // it's important to synchronize this property here
                // (because rollback might happen due to unable to spawn output items and container hash will be changed)
                // TODO: this is hack and we need a better way to track whether the container was actually changed or a better way to update the last state hash.
                manufacturingState.ContainerOutputLastStateHash
                    = manufacturingState.CraftingQueue.ContainerOutputLastStateHash;
            }
        }

        private void ServerRemote_SelectRecipe(IStaticWorldObject worldObject, Recipe recipe)
        {
            var character = ServerRemoteContext.Character;

            if (worldObject == null
                || worldObject.ProtoWorldObject != this
                || !this.SharedCanInteract(character, worldObject, writeToLog: true))
            {
                // player is too far from the world object or world object is destroyed
                return;
            }

            if (!recipe.SharedIsTechUnlocked(character))
            {
                throw new Exception("Cannot select locked recipe: " + recipe);
            }

            var statePrivate = GetPrivateState(worldObject);
            var manufacturingState = statePrivate.ManufacturingState;
            ManufacturingMechanic.SelectRecipe(
                recipe,
                worldObject,
                manufacturingState,
                this.ManufacturingConfig);

            // reset fuel container hash - to ensure the fuel will refresh as soon as possible
            if (statePrivate.FuelBurningState != null)
            {
                statePrivate.FuelBurningState.ContainerFuelLastStateHash = 0;
            }
        }
    }

    public abstract class ProtoObjectManufacturer
        : ProtoObjectManufacturer
            <ObjectManufacturerPrivateState,
                ObjectManufacturerPublicState,
                StaticObjectClientState>
    {
    }
}