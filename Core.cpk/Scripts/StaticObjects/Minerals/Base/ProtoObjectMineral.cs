namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Drones;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class ProtoObjectMineral
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoStaticWorldObject
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectMineral
        where TPrivateState : BasePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : DefaultMineralClientState, new()
    {
        public const string NotificationUsePickaxe = "Use a pickaxe to mine this resource.";

        private TextureAtlasResource textureAtlasResource;

        public override double ClientUpdateIntervalSeconds => 0.5;

        public ReadOnlyMineralDropItemsConfig DropItemsConfig { get; private set; }

        public virtual bool IsAllowDroneMining => true;

        public virtual bool IsAllowQuickMining => true;

        public override StaticObjectKind Kind => StaticObjectKind.NaturalObject;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public virtual string TextureAtlasPath => "StaticObjects/Minerals/" + this.GetType().Name;

        /// <summary>
        /// Gets the variants count of this object (the count of rows in atlas texture).
        /// </summary>
        public virtual byte TextureVariantsCount => 1;

        protected virtual bool CanFlipSprite => true;

        /// <summary>
        /// There are four damage stages - from 0 (not damaged) to 1 (slightly damaged)... to 4 (destroyed)
        /// </summary>
        protected virtual int DamageStagesCount => 4;

        public byte SharedCalculateDamageStage(float structurePoints)
        {
            var max = this.StructurePointsMax;
            structurePoints = MathHelper.Clamp(structurePoints, 0, max);
            return (byte)(this.DamageStagesCount * ((max - structurePoints) / max));
        }

        protected virtual ITextureResource ClientGetTextureResource(
            IStaticWorldObject gameObject,
            TPublicState publicState)
        {
            var damageStage = this.SharedCalculateDamageStage(publicState.StructurePointsCurrent);
            // select deterministically which sprites row in the atlas to use
            // (each row - different look for the same mineral)
            var rowIndex = (byte)PositionalRandom.Get(gameObject.TilePosition,
                                                      minInclusive: 0,
                                                      maxExclusive: this.TextureVariantsCount,
                                                      seed: 691237523);
            return this.textureAtlasResource.Chunk(damageStage, rowIndex);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            // don't use base implementation
            //base.ClientInitialize(data);

            this.ClientAddAutoStructurePointsBar(data);

            var gameObject = data.GameObject;
            var textureResource = this.ClientGetTextureResource(gameObject, data.PublicState);

            var spriteRenderer = Client.Rendering.CreateSpriteRenderer(
                gameObject,
                textureResource);

            data.ClientState.Renderer = spriteRenderer;
            this.ClientSetupRenderer(spriteRenderer);

            gameObject.ClientSceneObject
                      .AddComponent<ClientComponentObjectMineralStageWatcher>()
                      .Setup(gameObject, data.PublicState, this.ClientOnMineralStageChanged);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.5;

            var worldObject = renderer.SceneObject.AttachedWorldObject;
            if (worldObject is null)
            {
                return;
            }

            if (!this.CanFlipSprite)
            {
                return;
            }

            // flip renderer with some deterministic randomization
            if (PositionalRandom.Get(worldObject.TilePosition, 0, 2, seed: 925309274) == 0)
            {
                renderer.DrawMode = DrawMode.FlipHorizontally;
            }
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            this.textureAtlasResource = new TextureAtlasResource(
                this.TextureAtlasPath,
                4,
                rows: this.TextureVariantsCount,
                isTransparent: true);
            return this.textureAtlasResource;
        }

        protected abstract void PrepareProtoMineral(MineralDropItemsConfig config);

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            var dropItemsConfig = new MineralDropItemsConfig();
            this.PrepareProtoMineral(dropItemsConfig);
            this.DropItemsConfig = dropItemsConfig.AsReadOnly();
        }

        protected override void ServerOnStaticObjectDamageApplied(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            float previousStructurePoints,
            float currentStructurePoints)
        {
            try
            {
                var previousDamageStage = this.SharedCalculateDamageStage(previousStructurePoints);
                var currentDamageStage = this.SharedCalculateDamageStage(currentStructurePoints);
                var deltaDamageStages = currentDamageStage - previousDamageStage;

                if (deltaDamageStages <= 0)
                {
                    // damage stage not increased
                    return;
                }

                // damage stage increased!
                for (var damageStage = previousDamageStage + 1;
                     damageStage <= currentDamageStage;
                     damageStage++)
                {
                    // spawn items for damage stage
                    this.ServerOnMineralStageMined(weaponCache,
                                                   targetObject,
                                                   damageStage);
                }
            }
            finally
            {
                if ((weaponCache.ProtoWeapon is IProtoItemToolMining
                     || weaponCache.ProtoExplosive is ObjectBombMining)
                    && WorldObjectClaimSystem.SharedIsEnabled)
                {
                    this.ServerTryClaimObject(targetObject, weaponCache.Character);
                }

                base.ServerOnStaticObjectDamageApplied(weaponCache,
                                                       targetObject,
                                                       previousStructurePoints,
                                                       currentStructurePoints);
            }
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            if (!PveSystem.SharedIsAllowStaticObjectDamage(weaponCache.Character,
                                                           targetObject,
                                                           showClientNotification: false))
            {
                obstacleBlockDamageCoef = 0;
                return 0;
            }

            if (weaponCache.ProtoWeapon is IProtoItemToolMining protoItemToolMining)
            {
                // mining mineral with a mining device
                // block next damage completely - only one mineral could be mined at once
                obstacleBlockDamageCoef = 1;

                // get damage multiplier ("mining speed")
                var damageMultiplier = weaponCache.CharacterFinalStatsCache.GetMultiplier(StatName.MiningSpeed);

                return protoItemToolMining.ServerGetDamageToMineral(targetObject)
                       * damageMultiplier
                       * ToolsConstants.ActionMiningSpeedMultiplier;
            }

            if (weaponCache.ProtoWeapon is ItemNoWeapon)
            {
                // no damage with hands
                obstacleBlockDamageCoef = 1;

                if (IsClient)
                {
                    NotificationSystem.ClientShowNotification(NotificationUsePickaxe,
                                                              icon: this.Icon);
                }

                return 0;
            }

            // not a mining tool - call default damage apply method
            return base.SharedCalculateDamageByWeapon(weaponCache,
                                                      damagePreMultiplier,
                                                      targetObject,
                                                      out obstacleBlockDamageCoef);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 0.5),    offset: (0.0, 0.15))
                .AddShapeRectangle(size: (0.9, 0.8),  offset: (0.05, 0.1), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 0.15), offset: (0.1, 0.85), group: CollisionGroups.HitboxRanged)
                .AddShapeLineSegment(point1: (0.5, 0.2), point2: (0.5, 0.85), group: CollisionGroups.HitboxRanged);
        }

        private void ClientOnMineralStageChanged(IStaticWorldObject mineralObject)
        {
            var publicState = GetPublicState(mineralObject);
            var structurePoints = publicState.StructurePointsCurrent;
            var damageStage = this.SharedCalculateDamageStage(structurePoints);
            if (damageStage == this.DamageStagesCount)
            {
                // destroyed completely
                return;
            }

            var renderer = GetClientState(mineralObject).Renderer;
            renderer.TextureResource = this.ClientGetTextureResource(mineralObject, publicState);
        }

        private void ServerOnMineralStageMined(
            WeaponFinalCache weaponCache,
            IStaticWorldObject mineralObject,
            int damageStage)
        {
            var byCharacter = weaponCache.Character;
            var byWeaponProto = weaponCache.ProtoWeapon;

            Logger.Info(
                $"{mineralObject} current damage stage changed to {damageStage}. Dropping items for that stage",
                byCharacter);

            try
            {
                var dropItemsList = this.DropItemsConfig.GetForStage(damageStage);
                var dropItemContext = new DropItemContext(byCharacter,
                                                          mineralObject,
                                                          byWeaponProto,
                                                          weaponCache.ProtoExplosive);

                var objectDrone = weaponCache.Drone;
                if (objectDrone != null)
                {
                    // drop resources into the internal storage of the drone
                    var storageItemsContainer = ((IProtoDrone)objectDrone.ProtoGameObject)
                        .ServerGetStorageItemsContainer(objectDrone);
                    dropItemsList.TryDropToContainer(storageItemsContainer, dropItemContext);
                }
                else if (byWeaponProto is IProtoItemWeaponMelee)
                {
                    var result = dropItemsList.TryDropToCharacterOrGround(byCharacter,
                                                                          mineralObject.TilePosition,
                                                                          dropItemContext,
                                                                          groundContainer: out _);
                    if (result.TotalCreatedCount > 0)
                    {
                        NotificationSystem.ServerSendItemsNotification(byCharacter, result);
                    }
                }
                else
                {
                    // not a melee weapon or cannot drop to the character inventory - drop on the ground only
                    dropItemsList.TryDropToGround(mineralObject.TilePosition,
                                                  dropItemContext,
                                                  out _);
                }
            }
            finally
            {
                if (byWeaponProto is IProtoItemToolMining
                    || weaponCache.ProtoExplosive is ObjectBombMining)
                {
                    // add experience proportional to the mineral structure points (effectively - for the time spent on mining)
                    var exp = SkillProspecting.ExperienceAddPerStructurePoint;
                    exp *= this.StructurePointsMax / this.DamageStagesCount;
                    byCharacter?.ServerAddSkillExperience<SkillProspecting>(exp);
                }
            }
        }
    }

    public abstract class ProtoObjectMineral
        : ProtoObjectMineral
            <EmptyPrivateState, StaticObjectPublicState, DefaultMineralClientState>
    {
    }
}