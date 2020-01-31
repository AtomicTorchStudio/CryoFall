namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
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

        public virtual double GetGrowthStageDurationSeconds(byte growthStage)
        {
            return this.cachedGrowthStageDurationSeconds;
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
            if (textureAtlas == null)
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

            if (clientState.RendererShadow != null)
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
            return this.cachedGrowthStageDurationSeconds;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            if (data.IsFirstTimeInit)
            {
                this.ServerSetGrowthStage(data.GameObject,
                                          growthStage: this.TimeToMature.Ticks > 0
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

        protected override void ServerOnStaticObjectDestroyedByCharacter(
            ICharacter byCharacter,
            IProtoItemWeapon byWeaponProto,
            IStaticWorldObject targetObject)
        {
            base.ServerOnStaticObjectDestroyedByCharacter(byCharacter, byWeaponProto, targetObject);

            // drop chance and gained experience depends on the vegetation growth stage
            var growthProgressFraction = this.GrowthStagesCount > 0
                                             ? GetPublicState(targetObject).GrowthStage / (double)this.GrowthStagesCount
                                             : 1;

            growthProgressFraction = MathHelper.Clamp(growthProgressFraction, 0.1, 1);

            try
            {
                var dropItemContext = new DropItemContext(byCharacter, targetObject);
                if (byWeaponProto is IProtoItemWeaponMelee)
                {
                    // a melee weapon - try drop items to character
                    var result = this.DroplistOnDestroy.TryDropToCharacter(
                        byCharacter,
                        dropItemContext,
                        probabilityMultiplier: growthProgressFraction);
                    if (result.IsEverythingCreated)
                    {
                        NotificationSystem.ServerSendItemsNotification(byCharacter, result);
                        return;
                    }

                    result.Rollback();
                }

                // not a melee weapon or cannot drop to character - drop on the ground only
                this.DroplistOnDestroy.TryDropToGround(
                    targetObject.TilePosition,
                    dropItemContext,
                    probabilityMultiplier: growthProgressFraction,
                    groundContainer: out _);
            }
            finally
            {
                if (byWeaponProto is IProtoItemToolWoodcutting)
                {
                    // add experience proportional to the tree structure points (effectively - for the time spent on woodcutting)
                    var exp = SkillWoodcutting.ExperienceAddPerStructurePoint;
                    exp *= this.StructurePointsMax * growthProgressFraction;
                    byCharacter?.ServerAddSkillExperience<SkillWoodcutting>(exp);
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
    }
}