namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.CraftRecipes.Sprinkler;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Watering;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectSprinkler
        : ProtoObjectStructure
          <ProtoObjectSprinkler.PrivateState,
              ProtoObjectSprinkler.PublicState,
              StaticObjectClientState>,
          IProtoObjectSprinkler,
          IProtoObjectElectricityConsumerWithCustomRate
    {
        public const int WateringCooldownSeconds = 10; // cannot water more often than once in 10 seconds

        private const double WateringIntervalSeconds = 60 * 60; // once an hour

        private static readonly Lazy<SolidColorBrush> BlueprintBorderBrush
            = new Lazy<SolidColorBrush>(
                () => new SolidColorBrush(Color.FromArgb(0x99, 0x66, 0xCC, 0xFF)));

        private static readonly Lazy<SolidColorBrush> BlueprintFillBrush
            = new Lazy<SolidColorBrush>(
                () => new SolidColorBrush(Color.FromArgb(0x22, 0x66, 0xCC, 0xFF)));

        /// <summary>
        /// This object is not actually a manufacturer but it's more convenient to use the manufacturing mechanic
        /// as it provides containers for input and output, and a custom recipe could handle the water filling.
        /// </summary>
        private ManufacturingConfig manufacturingConfig;

        private ITextureAtlasResource textureAtlas;

        public virtual ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new ElectricityThresholdsPreset(startupPercent: 20,
                                               shutdownPercent: 10);

        // this value is actually never used as the consumption rate is always zero
        public double ElectricityConsumptionPerSecondWhenActive => 0.01;

        public abstract uint ElectricityConsumptionPerWatering { get; }

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override bool IsRelocatable => true;

        public override double ServerUpdateIntervalSeconds => 1;

        public abstract double WaterCapacity { get; }

        public abstract double WaterConsumptionPerWatering { get; }

        public abstract byte WateringDistance { get; }

        protected virtual byte ContainerInputSlotsCount => 1;

        protected virtual byte ContainerOutputSlotsCount => 1;

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            base.ClientSetupBlueprint(tile, blueprint);

            if (!ConstructionPlacementSystem.IsInObjectPlacementMode
                && !ConstructionRelocationSystem.IsInObjectPlacementMode)
            {
                return;
            }

            var bounds = this.Layout.Bounds;
            var sizeX = bounds.MaxX - bounds.MinX + 2 * this.WateringDistance;
            var sizeY = bounds.MaxY - bounds.MinY + 2 * this.WateringDistance;
            Api.Client.UI.AttachControl(
                blueprint.SceneObject,
                positionOffset: this.Layout.Center,
                uiElement: new Rectangle()
                {
                    Width = sizeX * ScriptingConstants.TileSizeVirtualPixels,
                    Height = sizeY * ScriptingConstants.TileSizeVirtualPixels,
                    Fill = BlueprintFillBrush.Value,
                    Stroke = BlueprintBorderBrush.Value,
                    StrokeThickness = 4,
                },
                isFocusable: false,
                isScaleWithCameraZoom: true);
        }

        public async void ClientWaterNow(IStaticWorldObject worldObjectSprinkler)
        {
            var result = await this.CallServer(_ => _.ServerRemote_WaterNow(worldObjectSprinkler));

            string errorMessage;
            switch (result)
            {
                case SprinklerWateringRequestResult.ErrorNotEnoughWater:
                case SprinklerWateringRequestResult.ErrorPowerOff:
                    errorMessage = result.GetDescription();
                    break;

                case SprinklerWateringRequestResult.ErrorNotEnoughElectricity:
                    errorMessage = PowerGridSystem.SetPowerModeResult.NotEnoughPower.GetDescription();
                    break;

                default:
                    return;
            }

            NotificationSystem.ClientShowNotification(WateringSystem.NotificationCannotWater_Title,
                                                      errorMessage,
                                                      NotificationColor.Bad,
                                                      icon: this.Icon);
        }

        public SprinklerWateringRequestResult ServerTryWaterNow(IStaticWorldObject worldObjectSprinkler)
        {
            var privateState = GetPrivateState(worldObjectSprinkler);
            var publicState = GetPublicState(worldObjectSprinkler);

            var checkResult = this.SharedCanWaterNow(worldObjectSprinkler);
            if (checkResult != SprinklerWateringRequestResult.Success)
            {
                Logger.Info("Sprinkler cannot water: " + checkResult,
                            characterRelated: ServerRemoteContext.IsRemoteCall
                                                  ? ServerRemoteContext.Character
                                                  : null);

                switch (checkResult)
                {
                    case SprinklerWateringRequestResult.ErrorNotEnoughWater:
                        privateState.NextWateringTime = double.MaxValue;
                        break;

                    case SprinklerWateringRequestResult.ErrorNotEnoughElectricity:
                        privateState.NextWateringTime = Server.Game.FrameTime + WateringIntervalSeconds;
                        break;
                }

                return checkResult;
            }

            privateState.LastWateringTime = Server.Game.FrameTime;
            privateState.NextWateringTime = Server.Game.FrameTime + WateringIntervalSeconds;

            var plantObjectsToWater = this.ServerGetPlantObjectsToWater(worldObjectSprinkler);
            if (plantObjectsToWater.Count == 0)
            {
                Logger.Info("No plants to water",
                            characterRelated: ServerRemoteContext.IsRemoteCall
                                                  ? ServerRemoteContext.Character
                                                  : null);
                return SprinklerWateringRequestResult.Success;
            }

            // consume water
            privateState.SetWaterAmount(privateState.WaterAmount - this.WaterConsumptionPerWatering,
                                        this.WaterCapacity,
                                        publicState);

            // consume electricity
            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(worldObjectSprinkler.Bounds);
            PowerGridSystem.ServerDeductBaseCharge(areasGroup, this.ElectricityConsumptionPerWatering);

            try
            {
                foreach (var worldObjectPlant in plantObjectsToWater)
                {
                    if (worldObjectPlant.ProtoGameObject is IProtoObjectPlant protoPlant)
                    {
                        protoPlant.ServerOnWatered(worldObjectPlant,
                                                   wateringDuration: WateringIntervalSeconds);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            Logger.Info("Watered surroundings now: " + worldObjectSprinkler);

            // reset hashes to ensure the recipe will refresh (it will refill the water bar if there are bottles with water)
            privateState.ManufacturingState.ContainerInputLastStateHash = null;
            privateState.ManufacturingState.ContainerOutputLastStateHash = null;

            return SprinklerWateringRequestResult.Success;
        }

        public SprinklerWateringRequestResult SharedCanWaterNow(IStaticWorldObject worldObjectSprinkler)
        {
            var time = IsServer
                           ? Server.Game.FrameTime
                           : Client.CurrentGame.ServerFrameTimeApproximated;

            var privateState = GetPrivateState(worldObjectSprinkler);
            if (privateState.LastWateringTime < double.MaxValue
                && time - privateState.LastWateringTime < WateringCooldownSeconds)
            {
                return SprinklerWateringRequestResult.ErrorWateredRecently;
            }

            if (GetPublicState(worldObjectSprinkler).ElectricityConsumerState == ElectricityConsumerState.PowerOff)
            {
                return SprinklerWateringRequestResult.ErrorPowerOff;
            }

            if (privateState.WaterAmount < this.WaterConsumptionPerWatering)
            {
                return SprinklerWateringRequestResult.ErrorNotEnoughWater;
            }

            if (IsServer)
            {
                var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(worldObjectSprinkler.Bounds);
                if (!PowerGridSystem.ServerBaseHasCharge(areasGroup, this.ElectricityConsumptionPerWatering))
                {
                    return SprinklerWateringRequestResult.ErrorNotEnoughElectricity;
                }
            }

            return SprinklerWateringRequestResult.Success;
        }

        public double SharedGetCurrentElectricityConsumptionRate(IStaticWorldObject worldObject)
        {
            // consumes electricity only when performing watering
            return 0;
        }

        IObjectElectricityStructurePrivateState IProtoObjectElectricityConsumer.GetPrivateState(
            IStaticWorldObject worldObject)
        {
            return GetPrivateState(worldObject);
        }

        IObjectElectricityConsumerPublicState IProtoObjectElectricityConsumer.GetPublicState(
            IStaticWorldObject worldObject)
        {
            return GetPublicState(worldObject);
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            var staticWorldObject = (IStaticWorldObject)worldObject;
            var privateState = GetPrivateState(staticWorldObject);
            return this.ClientOpenUI(staticWorldObject, privateState);
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var publicState = data.PublicState;
            var renderer = data.ClientState.Renderer;

            PowerGridSystem.ClientInitializeConsumerOrProducer(data.GameObject);

            publicState.ClientSubscribe(_ => _.WaterAmountPercent,
                                        _ => RefreshTexture(),
                                        data.ClientState);

            RefreshTexture();

            void RefreshTexture()
            {
                var columnIndex = (1 - publicState.WaterAmountPercent / 100.0)
                                  * (this.textureAtlas.AtlasSize.ColumnsCount - 1);
                renderer.TextureResource = this.textureAtlas.Chunk((byte)Math.Ceiling(columnIndex), 0);
            }
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected virtual BaseUserControlWithWindow ClientOpenUI(
            IStaticWorldObject worldObject,
            PrivateState privateState)
        {
            return WindowSprinkler.Open(
                new ViewModelWindowSprinkler(worldObject, privateState, this.manufacturingConfig));
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            tileRequirements.Clear()
                            .Add(ConstructionTileRequirements.ValidatorNoStaticObjectsExceptFloor)
                            .AddClientOnly(ConstructionTileRequirements.ValidatorClientOnlyNoCurrentPlayer)
                            .Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyDynamic)
                            .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                            .Add(ConstructionTileRequirements.ValidatorNoNpcsAround)
                            .Add(ConstructionTileRequirements.ValidatorNoPlayersNearby)
                            .Add(LandClaimSystem.ValidatorIsOwnedLandInPvEOnly)
                            .Add(LandClaimSystem.ValidatorNoRaid)
                            .Add(LandClaimSystem.ValidatorNoShieldProtection);

            this.PrepareConstructionConfigSprinkler(build,
                                                    repair,
                                                    upgrade,
                                                    out category);
        }

        protected abstract void PrepareConstructionConfigSprinkler(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category);

        protected sealed override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return this.textureAtlas = this.PrepareDefaultTextureAtlas(thisType);
        }

        protected abstract ITextureAtlasResource PrepareDefaultTextureAtlas(Type thisType);

        protected virtual void PrepareProtoObjectSprinkler()
        {
        }

        protected sealed override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.manufacturingConfig = new ManufacturingConfig(
                this,
                recipes: new[] { GetProtoEntity<RecipeSprinklerEmptyBottleFromWaterBottle>() },
                recipesForByproducts: null,
                isProduceByproducts: false,
                isAutoSelectRecipe: true);

            this.PrepareProtoObjectSprinkler();
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var worldObject = data.GameObject;
            var privateState = data.PrivateState;

            privateState.SetWaterAmount(privateState.WaterAmount,
                                        this.WaterCapacity,
                                        data.PublicState);

            // configure manufacturing state
            var manufacturingState = privateState.ManufacturingState;
            {
                var inputSlotsCount = this.ContainerInputSlotsCount;
                var outputSlotsCount = this.ContainerOutputSlotsCount;

                if (manufacturingState is null)
                {
                    manufacturingState = new ManufacturingState(
                        worldObject,
                        containerInputSlotsCount: inputSlotsCount,
                        containerOutputSlotsCount: outputSlotsCount);
                    privateState.ManufacturingState = manufacturingState;
                }
                else
                {
                    manufacturingState.SetSlotsCount(
                        input: inputSlotsCount,
                        output: outputSlotsCount);
                }

                Server.Items.SetContainerType<ItemsContainerWaterBottles>(
                    manufacturingState.ContainerInput);
                Server.Items.SetContainerType<ItemsContainerOutput>(
                    manufacturingState.ContainerOutput);
            }
        }

        protected virtual void ServerSelectTilesForWatering(IStaticWorldObject worldObject, List<Tile> tilesToWater)
        {
            var world = Server.World;
            var bounds = worldObject.Bounds;

            var rectangle = new RectangleInt(bounds.X, bounds.Y, bounds.Width, bounds.Height)
                .Inflate(this.WateringDistance);

            var rectangleRight = rectangle.Right;
            var rectangleTop = rectangle.Top;
            for (var x = rectangle.X; x < rectangleRight; x++)
            {
                for (var y = rectangle.Y; y < rectangleTop; y++)
                {
                    var tile = world.GetTile(x, y, logOutOfBounds: false);
                    tilesToWater.Add(tile);
                }
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var worldObject = data.GameObject;
            var privateState = data.PrivateState;
            var manufacturingState = privateState.ManufacturingState;

            // fill the water storage via the manufacturing mechanic
            ManufacturingMechanic.Update(
                worldObject,
                manufacturingState,
                this.manufacturingConfig,
                data.DeltaTime);

            if (privateState.NextWateringTime == double.MaxValue)
            {
                return;
            }

            if (data.PublicState.ElectricityConsumerState == ElectricityConsumerState.PowerOff)
            {
                privateState.NextWateringTime = double.MaxValue;
                return;
            }

            if (Server.Game.FrameTime < privateState.NextWateringTime)
            {
                return;
            }

            // time to water!
            if (data.PublicState.ElectricityConsumerState != ElectricityConsumerState.PowerOnActive)
            {
                // but the structure is idle - postpone till the next interval
                privateState.NextWateringTime = Server.Game.FrameTime + WateringIntervalSeconds;
                return;
            }

            this.ServerTryWaterNow(worldObject);
        }

        private HashSet<IStaticWorldObject> ServerGetPlantObjectsToWater(IStaticWorldObject worldObjectSprinkler)
        {
            using var tilesToWater = Api.Shared.GetTempList<Tile>();
            this.ServerSelectTilesForWatering(worldObjectSprinkler, tilesToWater.AsList());

            var plantObjectsToWater = new HashSet<IStaticWorldObject>();

            foreach (var tile in tilesToWater.AsList())
            {
                foreach (var tileObject in tile.StaticObjects)
                {
                    if (tileObject.ProtoGameObject is IProtoObjectPlant protoPlant
                        && protoPlant.ServerCanBeWatered(tileObject))
                    {
                        plantObjectsToWater.Add(tileObject);
                    }
                }
            }

            return plantObjectsToWater;
        }

        private SprinklerWateringRequestResult ServerRemote_WaterNow(IStaticWorldObject worldObject)
        {
            this.VerifyGameObject(worldObject);

            var character = ServerRemoteContext.Character;
            if (!this.SharedCanInteract(character, worldObject, writeToLog: true))
            {
                return SprinklerWateringRequestResult.ErrorNotInteracting;
            }

            return this.ServerTryWaterNow(worldObject);
        }

        public class PrivateState : StructurePrivateState, IObjectElectricityStructurePrivateState
        {
            [SyncToClient]
            public ElectricityThresholdsPreset ElectricityThresholds { get; set; }

            [SyncToClient]
            [TempOnly]
            public double LastWateringTime { get; set; } = double.MaxValue;

            [SyncToClient]
            public ManufacturingState ManufacturingState { get; set; }

            [SyncToClient]
            public double NextWateringTime { get; set; } = double.MaxValue;

            [SyncToClient]
            [TempOnly]
            public byte PowerGridChargePercent { get; set; }

            [SyncToClient(DeliveryMode.UnreliableSequenced, maxUpdatesPerSecond: 2, networkDataType: typeof(float))]
            public double WaterAmount { get; private set; }

            public void SetWaterAmount(double waterAmount, double waterCapacity, PublicState publicState)
            {
                waterAmount = MathHelper.Clamp(waterAmount, 0, waterCapacity);
                this.WaterAmount = waterAmount;
                publicState.WaterAmountPercent = (byte)(100 * waterAmount / waterCapacity);
            }
        }

        public class PublicState : StaticObjectElectricityConsumerPublicState
        {
            [SyncToClient]
            public byte WaterAmountPercent { get; set; }
        }
    }
}