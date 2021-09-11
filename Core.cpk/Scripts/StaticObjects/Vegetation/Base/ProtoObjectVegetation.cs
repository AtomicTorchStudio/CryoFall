namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Drones;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class ProtoObjectVegetation
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoStaticWorldObject
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectVegetation
        where TPrivateState : VegetationPrivateState, new()
        where TPublicState : VegetationPublicState, new()
        where TClientState : VegetationClientState, new()
    {
        private double cachedGrowthStageDurationSeconds;

        private byte cachedGrowthStagesCount;

        private double cachedTimeToGrowTotalSeconds;

        public override double ClientUpdateIntervalSeconds => 0.1;

        public IReadOnlyDropItemsList DroplistOnDestroy { get; private set; }

        public byte GrowthStagesCount => this.cachedGrowthStagesCount;

        public virtual bool IsAutoAddShadow => false;

        public override StaticObjectKind Kind => StaticObjectKind.NaturalObject;

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Vegetation;

        public override double ServerUpdateIntervalSeconds => 30.0;

        public virtual double StructurePointsRegenerationDurationSeconds => 30 * 60;

        protected virtual bool CanFlipSprite => true;

        protected abstract TimeSpan TimeToMature { get; }

        public virtual float CalculateShadowScale(VegetationClientState clientState)
        {
            return 0.33f
                   * (clientState.LastGrowthStage + 1)
                   / this.GrowthStagesCount;
        }

        public virtual byte ClientGetTextureAtlasColumn(
            IStaticWorldObject worldObject,
            VegetationPublicState statePublic)
        {
            return statePublic.GrowthStage;
        }

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            base.ClientSetupBlueprint(tile, blueprint);
            blueprint.SpriteRenderer.TextureResource = this.Icon;
        }

        /// <summary>
        /// Set vegetation to full grown state (by default all vegetation spawned as completely grown).
        /// </summary>
        /// <param name="worldObject"></param>
        public virtual void ServerSetFullGrown(IStaticWorldObject worldObject)
        {
            this.ServerSetGrowthStage(worldObject, this.GrowthStagesCount);
        }

        /// <summary>
        /// Sets growth progress.
        /// </summary>
        /// <param name="worldObject">Object with ProtoVegetation prototype.</param>
        /// <param name="progress">Value from 0 (not grown) to 1 (full grown)</param>
        public void ServerSetGrowthProgress(IStaticWorldObject worldObject, double progress)
        {
            progress = MathHelper.Clamp(progress, 0, 1);
            var growthStage = (byte)(this.GrowthStagesCount * progress);
            this.ServerSetGrowthStage(worldObject, growthStage);
        }

        public void ServerSetGrowthStage(IStaticWorldObject worldObject, byte growthStage)
        {
            var privateState = GetPrivateState(worldObject);
            var publicState = GetPublicState(worldObject);
            if (growthStage > this.GrowthStagesCount)
            {
                growthStage = this.GrowthStagesCount;
            }

            if (growthStage < this.GrowthStagesCount)
            {
                var serverTime = Api.Server.Game.FrameTime;
                var duration = this.ServerCalculateGrowthStageDuration(growthStage, privateState, publicState);
                privateState.ServerTimeNextGrowthStage = serverTime + duration;
            }

            publicState.GrowthStage = growthStage;
            //Logger.WriteDev(
            //    $"Vegetation growth stage set: {worldObject} growthStage={growthStage} nextStageDuration={duration:F2}s");
            this.ServerOnGrowthStageUpdated(worldObject, privateState, publicState);
        }

        public double SharedGetGrowthProgress(IWorldObject worldObject)
        {
            if (this.GrowthStagesCount <= 0)
            {
                return 1;
            }

            var publicState = GetPublicState((IStaticWorldObject)worldObject);
            return publicState.GrowthStage / (double)this.GrowthStagesCount;
        }

        protected virtual byte CalculateGrowthStagesCount()
        {
            return (byte)((this.DefaultTexture as ITextureAtlasResource)?.AtlasSize.ColumnsCount - 1 ?? 0);
        }

        protected virtual void ClientAddShadowRenderer(ClientInitializeData data)
        {
            var shadowScale = this.CalculateShadowScale(data.ClientState);
            data.ClientState.RendererShadow = ClientShadowHelper.AddShadowRenderer(
                data.GameObject,
                scaleMultiplier: shadowScale);
        }

        protected override ITextureResource ClientCreateIcon()
        {
            if (this.DefaultTexture is ITextureAtlasResource atlas)
            {
                // return last chunk
                return atlas.Chunk(
                    (byte)(atlas.AtlasSize.ColumnsCount - 1),
                    (byte)(atlas.AtlasSize.RowsCount - 1));
            }

            return this.DefaultTexture;
        }

        protected ITextureResource ClientGetTexture(
            IStaticWorldObject worldObject,
            VegetationPublicState publicState)
        {
            var textureResource = this.DefaultTexture;
            var textureAtlas = textureResource as TextureAtlasResource;
            if (textureAtlas is null)
            {
                // not a texture atlas - always use whole texture
                return textureResource;
            }

            var columnIndex = this.ClientGetTextureAtlasColumn(worldObject, publicState);
            return textureAtlas.Chunk(column: columnIndex, row: 0);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var gameObject = data.GameObject;
            var clientState = data.ClientState;
            var publicState = data.PublicState;

            clientState.LastGrowthStage = publicState.GrowthStage;

            var spriteRenderer = clientState.Renderer;
            spriteRenderer.TextureResource = this.ClientGetTexture(gameObject,
                                                                   publicState);

            clientState.Renderer = spriteRenderer;

            this.ClientRefreshVegetationRendering(data.GameObject, clientState, publicState);

            // flip renderer with some deterministic randomization
            if (this.CanFlipSprite
                && PositionalRandom.Get(gameObject.TilePosition, 0, 3, seed: 721886451) == 0)
            {
                spriteRenderer.DrawMode = DrawMode.FlipHorizontally;
            }

            if (this.IsAutoAddShadow)
            {
                this.ClientAddShadowRenderer(data);
            }
        }

        protected virtual void ClientRefreshVegetationRendering(
            IStaticWorldObject worldObject,
            VegetationClientState clientState,
            VegetationPublicState publicState)
        {
            clientState.LastGrowthStage = publicState.GrowthStage;
            clientState.Renderer.TextureResource = this.ClientGetTexture(worldObject, publicState);

            if (clientState.RendererShadow is not null)
            {
                clientState.RendererShadow.Scale = this.CalculateShadowScale(clientState);
            }
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            base.ClientUpdate(data);

            var clientState = data.ClientState;
            var publicState = data.PublicState;
            if (clientState.LastGrowthStage != publicState.GrowthStage)
            {
                this.ClientRefreshVegetationRendering(data.GameObject, clientState, publicState);
            }
        }

        protected abstract void PrepareDroplistOnDestroy(DropItemsList droplist);

        protected sealed override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.cachedTimeToGrowTotalSeconds = this.TimeToMature.TotalSeconds;

            var droplist = new DropItemsList();
            this.PrepareDroplistOnDestroy(droplist);
            this.DroplistOnDestroy = droplist.AsReadOnly();

            var textureColumnsCount = (this.DefaultTexture as ITextureAtlasResource)?.AtlasSize.ColumnsCount
                                      ?? 1;

            this.cachedGrowthStagesCount = this.CalculateGrowthStagesCount();

            Api.Assert(
                this.cachedGrowthStagesCount <= textureColumnsCount,
                "Texture atlas for " + this + " doesn't have enough grow stages.");

            // setup growth stage duration
            this.cachedGrowthStageDurationSeconds = this.cachedGrowthStagesCount > 0
                                                        ? this.cachedTimeToGrowTotalSeconds
                                                          / this.cachedGrowthStagesCount
                                                        : 0;

            this.PrepareProtoVegetation();
        }

        protected virtual void PrepareProtoVegetation()
        {
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return ObjectsSoundsPresets.ObjectVegetation;
        }

        protected virtual double ServerCalculateGrowthStageDuration(
            byte growthStage,
            TPrivateState privateState,
            TPublicState publicState)
        {
            var objectVegetation = privateState.GameObject;
            var duration = this.cachedGrowthStageDurationSeconds;
            if (LandClaimSystem.SharedIsObjectInsideAnyArea((IStaticWorldObject)objectVegetation))
            {
                // don't apply scaled rate to vegetation located inside the land claim areas
                // (as these are usually planted by players inside their bases and protected)
                return duration;
            }

            return ServerSpawnRateScaleHelper.AdjustDurationByRate(duration);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            if (data.IsFirstTimeInit)
            {
                this.ServerSetGrowthStage(data.GameObject,
                                          growthStage: this.cachedTimeToGrowTotalSeconds > 0
                                                           ? (byte)0
                                                           : this.GrowthStagesCount);
            }
        }

        protected virtual void ServerOnGrowthStageUpdated(
            IStaticWorldObject worldObject,
            TPrivateState privateState,
            TPublicState publicState)
        {
        }

        protected override void ServerOnStaticObjectDamageApplied(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            float previousStructurePoints,
            float currentStructurePoints)
        {
            var character = weaponCache.Character;
            if (character is not null
                && weaponCache.ProtoWeapon is not IProtoItemWeaponRanged
                && WorldObjectClaimSystem.SharedIsEnabled)
            {
                this.ServerTryClaimObject(targetObject, character);
            }

            base.ServerOnStaticObjectDamageApplied(weaponCache,
                                                   targetObject,
                                                   previousStructurePoints,
                                                   currentStructurePoints);
        }

        protected override void ServerOnStaticObjectDestroyedByCharacter(
            ICharacter byCharacter,
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject)
        {
            base.ServerOnStaticObjectDestroyedByCharacter(byCharacter, weaponCache, targetObject);

            // drop chance and gained experience depends on the vegetation growth stage
            var growthProgressFraction = this.SharedGetGrowthProgress(targetObject);
            growthProgressFraction = MathHelper.Clamp(growthProgressFraction, 0.1, 1);

            try
            {
                var dropItemsList = this.DroplistOnDestroy;
                var dropItemContext = new DropItemContext(byCharacter,
                                                          targetObject,
                                                          weaponCache.ProtoWeapon,
                                                          weaponCache.ProtoExplosive);

                var objectDrone = weaponCache.Drone;
                var probabilityMultiplier = growthProgressFraction
                                            * RateResourcesGatherBasic.SharedValue;

                if (objectDrone is not null)
                {
                    // drop resources into the internal storage of the drone
                    var storageItemsContainer = ((IProtoDrone)objectDrone.ProtoGameObject)
                        .ServerGetStorageItemsContainer(objectDrone);
                    dropItemsList.TryDropToContainer(
                        storageItemsContainer,
                        dropItemContext,
                        probabilityMultiplier: probabilityMultiplier);
                }
                else if (weaponCache.ProtoWeapon is IProtoItemWeaponMelee)
                {
                    // a melee weapon - try drop items to character
                    var result = dropItemsList.TryDropToCharacterOrGround(
                        byCharacter,
                        targetObject.TilePosition,
                        dropItemContext,
                        groundContainer: out _,
                        probabilityMultiplier: probabilityMultiplier);
                    if (result.TotalCreatedCount > 0)
                    {
                        NotificationSystem.ServerSendItemsNotification(byCharacter, result);
                    }
                }
                else
                {
                    // not a melee weapon or cannot drop to character - drop on the ground only
                    dropItemsList.TryDropToGround(
                        targetObject.TilePosition,
                        dropItemContext,
                        probabilityMultiplier: probabilityMultiplier,
                        groundContainer: out _);
                }
            }
            finally
            {
                if (weaponCache.ProtoWeapon is IProtoItemToolWoodcutting)
                {
                    // add experience proportional to the vegetation structure points
                    // (effectively - for the time spent on woodcutting)
                    var exp = SkillLumbering.ExperienceAddPerStructurePoint;
                    exp *= this.StructurePointsMax * growthProgressFraction;
                    byCharacter?.ServerAddSkillExperience<SkillLumbering>(exp);
                }
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            var structurePoints = publicState.StructurePointsCurrent;
            if (structurePoints < this.StructurePointsMax
                && this.StructurePointsRegenerationDurationSeconds > 0
                && !Server.World.IsObservedByAnyPlayer(data.GameObject))
            {
                // regenerate vegetation health points (only if it's not observed by any player)
                var regenerationPointsPerSecond =
                    this.StructurePointsMax / this.StructurePointsRegenerationDurationSeconds;

                structurePoints =
                    Math.Min((float)(structurePoints + data.DeltaTime * regenerationPointsPerSecond),
                             this.StructurePointsMax);

                publicState.StructurePointsCurrent = structurePoints;
            }

            // update growth
            if (publicState.GrowthStage == this.GrowthStagesCount)
            {
                // vegetation is full grown
                return;
            }

            // the vegetation is not full grown - need to update its growth progress
            if (privateState.ServerTimeNextGrowthStage > Api.Server.Game.FrameTime)
            {
                // not yet grown
                return;
            }

            // increase growth stage
            this.ServerSetGrowthStage(data.GameObject, (byte)(publicState.GrowthStage + 1));
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            var damage = base.SharedCalculateDamageByWeapon(weaponCache,
                                                            damagePreMultiplier,
                                                            targetObject,
                                                            out obstacleBlockDamageCoef);
            if (damage <= 0)
            {
                return damage;
            }

            return damage * this.SharedGetDamageMultiplierByGrowthProgress(targetObject);
        }

        /// <summary>
        /// Not fully grown trees should be cut faster (linearly proportional to their growth progress).
        /// </summary>
        protected double SharedGetDamageMultiplierByGrowthProgress(IStaticWorldObject targetObject)
        {
            var growthProgressFraction = this.SharedGetGrowthProgress(targetObject);
            growthProgressFraction = MathHelper.Clamp(growthProgressFraction, 0.25, 1);
            return 1 / growthProgressFraction;
        }

        protected override bool SharedIsAllowedObjectToInteractThrough(IWorldObject worldObject)
        {
            return true;
        }
    }
}