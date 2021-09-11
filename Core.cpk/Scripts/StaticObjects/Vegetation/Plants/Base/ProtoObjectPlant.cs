namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Plants;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public abstract class ProtoObjectPlant
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectGatherableVegetation<TPrivateState, TPublicState, TClientState>,
          IProtoObjectPlant
        where TPrivateState : PlantPrivateState, new()
        where TPublicState : PlantPublicState, new()
        where TClientState : PlantClientState, new()
    {
        public const string ErrorRequiresFarmPlot = "Requires farm plot.";

        private IComponentAttachedControl currentDisplayedTooltip;

        private double stageTimeToMatureTotalSeconds;

        public ITextureResource IconFullGrown => this.Icon;

        public override bool IsAutoDestroyOnGather => false;

        public abstract byte NumberOfHarvests { get; }

        public override double ServerUpdateIntervalSeconds => 5;

        public IReadOnlyDropItemsList SpoiledGatherDroplist { get; private set; }

        public double TimeToGiveHarvestTotalSeconds { get; private set; }

        public double TimeToHarvestSpoilTotalSeconds { get; private set; }

        protected byte GrowthStageIndexHarvestProductionProcess => (byte)(this.GrowthStagesCount - 2);

        protected byte GrowthStageIndexSpoilingProcess => (byte)(this.GrowthStagesCount - 1);

        protected abstract TimeSpan TimeToGiveHarvest { get; }

        protected virtual TimeSpan TimeToHarvestSpoil { get; } = TimeSpan.FromHours(32);

        public override float CalculateShadowScale(VegetationClientState clientState)
        {
            // no shadows for plants (all shadows are pre-drawn)
            return 0;
        }

        public double ClientCalculateHarvestTotalDuration(bool onlyForHarvestStage)
        {
            var result = this.TimeToGiveHarvestTotalSeconds;
            if (!onlyForHarvestStage)
            {
                // add all growth stages duration (stages prior to giving the harvest)
                result += this.stageTimeToMatureTotalSeconds * this.GrowthStageIndexHarvestProductionProcess;
            }

            return result;
        }

        public override byte ClientGetTextureAtlasColumn(
            IStaticWorldObject worldObject,
            VegetationPublicState statePublic)
        {
            var plantPublicState = (TPublicState)statePublic;
            if (plantPublicState.IsSpoiled)
            {
                return (byte)(this.GrowthStageIndexSpoilingProcess + 1);
            }

            if (plantPublicState.HasHarvest)
            {
                return (byte)(this.GrowthStageIndexHarvestProductionProcess + 1);
            }

            return base.ClientGetTextureAtlasColumn(worldObject, statePublic);
        }

        public override string ClientGetTitle(IWorldObject worldObject)
        {
            // title is already included into the plant tooltip
            return null;
        }

        public Task<ProtoPlantTooltipPrivateData> ClientGetTooltipData(IStaticWorldObject plant)
        {
            Api.Assert(plant is not null, "Plant object cannot be null");
            return this.CallServer(_ => _.ServerRemote_GetTooltipData(plant));
        }

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            base.ClientSetupBlueprint(tile, blueprint);
            ClientAddPlantRenderingOffsetFromFarm(tile, blueprint.SpriteRenderer);
        }

        public override double GetGatheringSpeedMultiplier(IStaticWorldObject worldObject, ICharacter character)
        {
            return character.SharedGetFinalStatMultiplier(StatName.FarmingTasksSpeed);
        }

        public virtual bool ServerCanBeWatered(IStaticWorldObject worldObjectPlant)
        {
            var plantPublicState = GetPublicState(worldObjectPlant);
            if (plantPublicState.IsSpoiled)
            {
                // Please note: it's possible to water a plant that is ready to harvest
                // (useful to reset the spoil timer for flowers).
                return false;
            }

            return this.ServerGetWateringFraction(worldObjectPlant) < 1;
        }

        public double ServerGetWateringFraction(IStaticWorldObject worldObjectPlant)
        {
            var privateState = GetPrivateState(worldObjectPlant);
            return ServerTimersSystem.SharedGetTimeRemainingFraction(
                privateState.ServerTimeWateringEnds,
                privateState.LastWateringDuration,
                out _);
        }

        public void ServerOnWatered(
            IStaticWorldObject worldObjectPlant,
            double wateringDuration,
            [CanBeNull] ICharacter byCharacter = null)
        {
            var privateState = GetPrivateState(worldObjectPlant);
            var publicState = GetPublicState(worldObjectPlant);

            var previousWateringFraction = this.ServerGetWateringFraction(worldObjectPlant);

            privateState.LastWateringDuration = wateringDuration;
            if (wateringDuration < double.MaxValue)
            {
                privateState.ServerTimeWateringEnds = Server.Game.FrameTime + wateringDuration;
            }
            else
            {
                privateState.ServerTimeWateringEnds = double.MaxValue;
            }

            if (publicState.IsWatered)
            {
                publicState.ServerForceIsWateredSync();
            }
            else
            {
                publicState.IsWatered = true;
            }

            this.ServerRefreshCurrentGrowthDuration(worldObjectPlant);

            var newWateringFraction = ServerTimersSystem.SharedGetTimeRemainingFraction(
                privateState.ServerTimeWateringEnds,
                privateState.LastWateringDuration,
                out _);

            if (publicState.HasHarvest)
            {
                // reset the spoil timer
                // calculate the actual growth duration (with current bonuses)
                var duration =
                    this.ServerCalculateGrowthStageDuration(publicState.GrowthStage, privateState, publicState);

                // calculate and apply the remaining duration (with current bonuses)
                privateState.ServerTimeNextGrowthStage = Server.Game.FrameTime + duration;
                privateState.ServerTimeLastDurationSeconds = (float)duration;
            }

            var wateringFractionAdded = newWateringFraction - previousWateringFraction;
            wateringFractionAdded = MathHelper.Clamp(wateringFractionAdded, 0, 1);

            byCharacter?.ServerAddSkillExperience<SkillFarming>(
                SkillFarming.ExperienceForWatering * wateringFractionAdded);
        }

        public void ServerRefreshCurrentGrowthDuration(IStaticWorldObject worldObjectPlant)
        {
            var publicState = GetPublicState(worldObjectPlant);
            if (publicState.HasHarvest)
            {
                // already has harvest - no need to refresh the growth time
                return;
            }

            var serverTime = Api.Server.Game.FrameTime;
            var privateState = GetPrivateState(worldObjectPlant);

            // calculate the remaining duration
            var previousDuration = (double)privateState.ServerTimeLastDurationSeconds;
            var remainingDuration = privateState.ServerTimeNextGrowthStage - serverTime;
            remainingDuration = MathHelper.Clamp(remainingDuration, min: 0, max: previousDuration);

            // calculate the current stage growth progress ([0;1] range where 0 == no progress; 1 == completed)
            var progressMultiplier = remainingDuration / previousDuration;

            // calculate the actual growth duration (with current bonuses)
            var duration = this.ServerCalculateGrowthStageDuration(publicState.GrowthStage, privateState, publicState);

            // calculate and apply the remaining duration (with current bonuses)
            remainingDuration = duration * progressMultiplier;
            privateState.ServerTimeNextGrowthStage = serverTime + remainingDuration;
            privateState.ServerTimeLastDurationSeconds = (float)duration;

            //Logger.Write(
            //    $"Current stage growth duration updated: {worldObjectPlant}: from {previousDuration:F1}s to {remainingDuration:F1}s");
        }

        public void ServerSetBonusForCharacter(IStaticWorldObject plantObject, ICharacter character)
        {
            this.ServerSetBonusForCharacter(plantObject, character, applyNow: true);
        }

        public void ServerSetBonusForCharacter(IStaticWorldObject plantObject, ICharacter character, bool applyNow)
        {
            var multiplier = character.SharedGetFinalStatMultiplier(StatName.FarmingPlantGrowSpeed);
            GetPrivateState(plantObject).SkillGrowthSpeedMultiplier = multiplier;
            if (applyNow)
            {
                this.ServerRefreshCurrentGrowthDuration(plantObject);
            }
        }

        public override void ServerSetFullGrown(IStaticWorldObject worldObject)
        {
            this.ServerSetGrowthStage(worldObject, this.GrowthStageIndexSpoilingProcess);
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            return PveSystem.SharedValidateInteractionIsNotForbidden(character, worldObject, writeToLog)
                   && NewbieProtectionSystem.SharedValidateInteractionIsNotForbidden(character, worldObject, writeToLog)
                   && this.SharedIsInsideCharacterInteractionArea(character, worldObject, writeToLog);
        }

        public override bool SharedIsCanGather(IStaticWorldObject staticWorldObject)
        {
            var publicState = GetPublicState(staticWorldObject);
            return publicState.HasHarvest
                   || publicState.IsSpoiled;
        }

        protected static IProtoObjectFarm CommonGetFarmObjectProto(Tile tile)
        {
            var farmObject = tile.StaticObjects.FirstOrDefault(
                _ => _.ProtoStaticWorldObject is IProtoObjectFarm);
            return (IProtoObjectFarm)farmObject?.ProtoStaticWorldObject;
        }

        protected override byte CalculateGrowthStagesCount()
        {
            return (byte)((this.DefaultTexture as ITextureAtlasResource)?.AtlasSize.ColumnsCount - 1 ?? 0);
        }

        protected override void ClientAddShadowRenderer(ClientInitializeData data)
        {
            var farmObject = CommonGetFarmObjectProto(data.GameObject.OccupiedTile);
            if (farmObject is not null
                && !farmObject.IsDrawingPlantShadow)
            {
                // don't create plant shadow renderer
                return;
            }

            base.ClientAddShadowRenderer(data);
        }

        protected override ITextureResource ClientCreateIcon()
        {
            return this.DefaultTexture is ITextureAtlasResource atlas
                       ? atlas.Chunk(this.GrowthStageIndexSpoilingProcess, 0)
                       : this.DefaultTexture;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;
            var publicState = data.PublicState;
            var clientState = data.ClientState;
            var tile = worldObject.OccupiedTile;
            ClientAddPlantRenderingOffsetFromFarm(tile, clientState.Renderer);

            var rendererShadow = clientState.RendererShadow;
            if (rendererShadow is not null)
            {
                ClientAddPlantRenderingOffsetFromFarm(tile, rendererShadow);
            }

            // force reinitialize the farm plot to ensure it correctly uses the watered state of the plant over it
            SharedGetFarmPlotWorldObject(worldObject.OccupiedTile)?.ClientInitialize();

            publicState.ClientSubscribe(_ => _.IsWatered,
                                        _ => SharedGetFarmPlotWorldObject(worldObject.OccupiedTile)
                                            ?.ClientInitialize(),
                                        clientState);

            publicState.ClientSubscribe(_ => _.IsFertilized,
                                        _ => SharedGetFarmPlotWorldObject(worldObject.OccupiedTile)
                                            ?.ClientInitialize(),
                                        clientState);
        }

        protected override void ClientObserving(ClientObjectData data, bool isObserving)
        {
            if (!isObserving)
            {
                this.currentDisplayedTooltip.Destroy();
                this.currentDisplayedTooltip = null;
                return;
            }

            var worldObject = data.GameObject;
            var control = new FarmPlantTooltip
            {
                ObjectPlant = worldObject,
                PlantPublicState = data.PublicState,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            this.currentDisplayedTooltip = Client.UI.AttachControl(
                worldObject,
                control,
                positionOffset: this.SharedGetObjectCenterWorldOffset(
                                    worldObject)
                                + (0, 1.175),
                isFocusable: false);
        }

        protected sealed override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // empty destroy droplist by default
        }

        protected sealed override void PrepareProtoGatherableVegetation()
        {
            this.TimeToGiveHarvestTotalSeconds = this.TimeToGiveHarvest.TotalSeconds;
            this.TimeToHarvestSpoilTotalSeconds = this.TimeToHarvestSpoil.TotalSeconds;
            this.stageTimeToMatureTotalSeconds = this.TimeToMature.TotalSeconds
                                                 / this.GrowthStageIndexSpoilingProcess;

            var spoiledGatherDroplist = new DropItemsList();
            this.PrepareSpoiledGatheringDroplist(spoiledGatherDroplist);
            this.SpoiledGatherDroplist = spoiledGatherDroplist.AsReadOnly();

            this.PrepareProtoPlant();
        }

        protected virtual void PrepareProtoPlant()
        {
        }

        protected virtual void PrepareSpoiledGatheringDroplist(DropItemsList spoiledGatherDroplist)
        {
            spoiledGatherDroplist.Add<ItemFibers>(count: 3, countRandom: 2);
        }

        protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
        {
            tileRequirements
                .Clear()
                .Add(ErrorRequiresFarmPlot,
                     c => c.Tile.StaticObjects.Any(_ => _.ProtoStaticWorldObject is IProtoObjectFarm))
                .Add(ConstructionTileRequirements.BasicRequirements)
                .Add(ConstructionTileRequirements.ErrorNoFreeSpace,
                     // ensure there are no static physics objects (other than farm)
                     c => !ConstructionTileRequirements
                              .TileHasAnyPhysicsObjectsWhere(
                                  c.Tile,
                                  o => o.PhysicsBody.IsStatic
                                       && o.PhysicsBody.AssociatedWorldObject?.ProtoWorldObject
                                           is not IProtoObjectFarm));
        }

        protected override double ServerCalculateGrowthStageDuration(
            byte growthStage,
            TPrivateState privateState,
            TPublicState publicState)
        {
            var speedMultiplier = ServerCalculateGrowthSpeedMultiplier(privateState, publicState);
            if (growthStage < this.GrowthStageIndexHarvestProductionProcess)
            {
                // not full grown yet
                return this.stageTimeToMatureTotalSeconds / speedMultiplier;
            }

            if (growthStage == this.GrowthStageIndexHarvestProductionProcess)
            {
                // next stage is harvest production
                return this.TimeToGiveHarvestTotalSeconds / speedMultiplier;
            }

            // next stage is spoiling (the growth speed multiplier variable is not applicable here)
            return this.TimeToHarvestSpoilTotalSeconds
                   * FarmingConstants.SharedFarmPlantsLifetimeMultiplier;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            data.PublicState.IsFertilized = data.PrivateState.AppliedFertilizerProto is not null;
        }

        protected override void ServerOnGathered(IStaticWorldObject worldObject, ICharacter byCharacter)
        {
            byCharacter.ServerAddSkillExperience<SkillFarming>(SkillFarming.ExperienceForHarvesting);
        }

        protected override void ServerOnGrowthStageUpdated(
            IStaticWorldObject worldObject,
            TPrivateState privateState,
            TPublicState publicState)
        {
            // store current growth duration
            privateState.ServerTimeLastDurationSeconds =
                (float)(privateState.ServerTimeNextGrowthStage - Server.Game.FrameTime);

            if (publicState.GrowthStage < this.GrowthStageIndexSpoilingProcess)
            {
                return;
            }

            if (publicState.GrowthStage == this.GrowthStageIndexSpoilingProcess)
            {
                // produce harvest
                publicState.HasHarvest = true;
                if (this.NumberOfHarvests > 0)
                {
                    privateState.ProducedHarvestsCount++;
                }
            }
            else
            {
                publicState.IsSpoiled = true;
            }
        }

        protected override void ServerTryClaimObject(IStaticWorldObject targetObject, ICharacter character)
        {
            // don't apply claim on farm plants
        }

        protected override bool ServerTryGatherByCharacter(ICharacter who, IStaticWorldObject vegetationObject)
        {
            var publicState = GetPublicState(vegetationObject);
            if (!publicState.IsSpoiled)
            {
                if (!base.ServerTryGatherByCharacter(who, vegetationObject))
                {
                    return false;
                }
            }
            else // spoiled plant
            {
                var result = this.SpoiledGatherDroplist.TryDropToCharacterOrGround(
                    who,
                    who.TilePosition,
                    new DropItemContext(who, vegetationObject),
                    out var groundItemsContainer,
                    probabilityMultiplier: RateResourcesGatherBasic.SharedValue);
                if (result.TotalCreatedCount == 0)
                {
                    result.Rollback();
                    return false;
                }

                Logger.Info(vegetationObject + " was gathered when spoiled", who);
                NotificationSystem.ServerSendItemsNotification(
                    who,
                    result,
                    exceptItemsContainer: groundItemsContainer);
            }

            // reset grown harvest state
            this.ServerClearHarvestState(vegetationObject, who);
            return true;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            if (publicState.IsWatered
                && Server.Game.FrameTime >= privateState.ServerTimeWateringEnds)
            {
                // watering ended
                publicState.IsWatered = false;
                this.ServerRefreshCurrentGrowthDuration(data.GameObject);
            }

            base.ServerUpdate(data);
        }

        protected override void SharedProcessCreatedPhysics(CreatePhysicsData data)
        {
            base.SharedProcessCreatedPhysics(data);
            if (!data.GameObject.OccupiedTile.StaticObjects.Any(o => o.ProtoStaticWorldObject is IProtoObjectPlantPot))
            {
                return;
            }

            // the plant is in a plant pot - remove collision groups
            data.PhysicsBody.RemoveShapesOfGroup(CollisionGroups.Default);

            // TODO: actually, we should not remove the hitboxes - just offset their position
            data.PhysicsBody.RemoveShapesOfGroup(CollisionGroups.HitboxMelee);
            data.PhysicsBody.RemoveShapesOfGroup(CollisionGroups.HitboxRanged);
        }

        private static void ClientAddPlantRenderingOffsetFromFarm(Tile tile, IComponentSpriteRenderer renderer)
        {
            var protoFarmObject = CommonGetFarmObjectProto(tile);
            if (protoFarmObject is null)
            {
                // no farm
                return;
            }

            var drawOffset = protoFarmObject.PlacedPlantPositionOffset;
            if (drawOffset != Vector2D.Zero)
            {
                renderer.PositionOffset += drawOffset;
            }
        }

        private static double ServerCalculateGrowthSpeedMultiplier(
            TPrivateState privateState,
            TPublicState publicState)
        {
            var multiplier = 1.0;

            if (privateState.AppliedFertilizerProto is not null)
            {
                // apply fertilizer effect (as percents)
                multiplier += Math.Max(0, privateState.AppliedFertilizerProto.PlantGrowthSpeedMultiplier - 1);
            }

            if (publicState.IsWatered)
            {
                // apply watering effect (as percents)
                multiplier += FarmingConstants.WateringGrowthSpeedMultiplier - 1;
            }

            // apply skill growth speed multiplier (as percents)
            multiplier += Math.Max(0, privateState.SkillGrowthSpeedMultiplier - 1);

            // apply growth rate multiplier
            multiplier *= FarmingConstants.ServerFarmPlantsGrowthSpeedMultiplier;

            return multiplier;
        }

        private static IStaticWorldObject SharedGetFarmPlotWorldObject(Tile tile)
        {
            return tile.StaticObjects.FirstOrDefault(
                o => o.ProtoStaticWorldObject is IProtoObjectFarmPlot);
        }

        private double ServerCalculateTotalGrowthTimeToNextHarvest(
            TPrivateState privateState,
            TPublicState publicState)
        {
            var currentGrowthStage = publicState.GrowthStage;
            if (currentGrowthStage >= this.GrowthStageIndexSpoilingProcess)
            {
                // full grown
                return 0;
            }

            // get current time for next growth stage
            var result = privateState.ServerTimeNextGrowthStage;

            // add durations of all the next growth stages
            var growthStage = (byte)(currentGrowthStage + 1);
            while (growthStage < this.GrowthStageIndexSpoilingProcess)
            {
                result += this.ServerCalculateGrowthStageDuration(growthStage, privateState, publicState);
                growthStage++;
            }

            return result;
        }

        private void ServerClearHarvestState(IStaticWorldObject worldObject, ICharacter gatheredByCharacter)
        {
            var publicState = GetPublicState(worldObject);
            var privateState = GetPrivateState(worldObject);

            if (publicState.IsSpoiled)
            {
                Server.World.DestroyObject(worldObject);
                return;
            }

            if (!publicState.HasHarvest)
            {
                return;
            }

            publicState.HasHarvest = false;

            if (this.NumberOfHarvests > 0
                && privateState.ProducedHarvestsCount >= this.NumberOfHarvests)
            {
                Server.World.DestroyObject(worldObject);
                return;
            }

            // set previous growth stage (on achieving last growth stage it will produce the harvest)
            if (gatheredByCharacter is not null)
            {
                this.ServerSetBonusForCharacter(worldObject, gatheredByCharacter, applyNow: false);
            }

            this.ServerSetGrowthStage(worldObject, this.GrowthStageIndexHarvestProductionProcess);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 0.25)]
        private ProtoPlantTooltipPrivateData ServerRemote_GetTooltipData(IStaticWorldObject worldObjectPlant)
        {
            this.VerifyGameObject(worldObjectPlant);

            using (var scopedBy = Api.Shared.GetTempList<ICharacter>())
            {
                Server.World.GetScopedByPlayers(worldObjectPlant, scopedBy);
                if (!scopedBy.Contains(ServerRemoteContext.Character))
                {
                    throw new Exception("Character doesn't has the plant in its scope");
                }
            }

            var privateState = GetPrivateState(worldObjectPlant);
            var publicState = GetPublicState(worldObjectPlant);
            var speedMultiplier = ServerCalculateGrowthSpeedMultiplier(privateState, publicState);

            double serverTimeNextHarvestOrSpoil;
            if (publicState.IsSpoiled)
            {
                // already rot
                serverTimeNextHarvestOrSpoil = 0;
            }
            else if (publicState.HasHarvest)
            {
                // time to rot
                serverTimeNextHarvestOrSpoil = privateState.ServerTimeNextGrowthStage;
            }
            else
            {
                // time to give harvest
                serverTimeNextHarvestOrSpoil = this.ServerCalculateTotalGrowthTimeToNextHarvest(
                    privateState,
                    publicState);
            }

            return new ProtoPlantTooltipPrivateData(privateState,
                                                    serverTimeNextHarvestOrSpoil,
                                                    (float)speedMultiplier);
        }
    }

    public abstract class ProtoObjectPlant
        : ProtoObjectPlant<
            PlantPrivateState,
            PlantPublicState,
            PlantClientState>
    {
    }
}