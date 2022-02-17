namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.RaidingProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectExplosive
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoStaticWorldObject
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectExplosive
        where TPrivateState : ObjectExplosivePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        protected static readonly Lazy<SolidColorBrush> ExplosionBlueprintBorderBrush
            = new(() => new SolidColorBrush(Color.FromArgb(0x99, 0xEE, 0x44, 0x00)));

        protected static readonly Lazy<RadialGradientBrush> ExplosionBlueprintFillBrush
            = new(() => new RadialGradientBrush()
            {
                GradientStops = new GradientStopCollection()
                {
                    new() { Color = Color.FromArgb(0x20, 0xEE, 0x44, 0x00), Offset = 1 },
                    new() { Color = Color.FromArgb(0x3C, 0xEE, 0x44, 0x00), Offset = 0 }
                }
            });

        private DamageDescription damageDescriptionCharacters;

        public virtual bool AllowNpcToNpcDamage => false;

        public DamageDescription DamageDescriptionCharacters => this.damageDescriptionCharacters;

        public abstract double DamageRadius { get; }

        public abstract TimeSpan ExplosionDelay { get; }

        public ExplosionPreset ExplosionPreset { get; private set; }

        public override bool HasIncreasedScopeSize => true;

        public abstract bool IsActivatesRaidBlock { get; }

        public virtual bool IsDamageThroughObstacles => false;

        /// <summary>
        /// Does the explosive object should detonate immediately (skip the delay) when damaged?
        /// It makes sense to keep it "true" for bombs and set it to "false" for some environmental explosions.
        /// </summary>
        public abstract bool IsExplosionDelaySkippedOnDamage { get; }

        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public sealed override double ObstacleBlockDamageCoef => 0;

        public virtual double RaidBlockDurationMutliplier => 1.0;

        public sealed override double ServerUpdateIntervalSeconds => 0; // every frame

        public double StructureDamage { get; private set; }

        public double StructureDefensePenetrationCoef { get; private set; }

        public sealed override float StructurePointsMax => 9001; // it's non-damageable anyway

        // has a massive light source
        public override BoundsInt ViewBoundsExpansion => new(minX: -12,
                                                             minY: -12,
                                                             maxX: 12,
                                                             maxY: 12);

        public virtual float VolumeExplosion => 1;

        public sealed override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            base.ClientSetupBlueprint(tile, blueprint);
            this.ClientSetupExplosionBlueprint(tile, blueprint);
        }

        public virtual double ServerCalculateDamageCoefByDistanceForDynamicObjects(double distance)
        {
            var distanceThreshold = 1;
            if (distance <= distanceThreshold)
            {
                return 1;
            }

            distance -= distanceThreshold;
            distance = Math.Max(0, distance);

            var maxDistance = this.DamageRadius;
            maxDistance -= distanceThreshold;
            maxDistance = Math.Max(0, maxDistance);

            return 1 - Math.Min(distance / maxDistance, 1);
        }

        public virtual double ServerCalculateDamageCoefByDistanceForStaticObjects(double distance)
        {
            distance -= 1.42;
            distance = Math.Max(0, distance);
            return 1 - (distance / this.DamageRadius);
        }

        public virtual double ServerCalculateTotalDamageByExplosive(
            ICharacter byCharacter,
            IStaticWorldObject targetStaticWorldObject)
        {
            var structureExplosiveDefenseCoef =
                targetStaticWorldObject.ProtoStaticWorldObject.StructureExplosiveDefenseCoef;
            structureExplosiveDefenseCoef = MathHelper.Clamp(structureExplosiveDefenseCoef, 0, 1);

            var explosiveDefensePenetrationCoef = this.StructureDefensePenetrationCoef;
            explosiveDefensePenetrationCoef = MathHelper.Clamp(explosiveDefensePenetrationCoef, 0, 1);

            if (!PveSystem.SharedIsAllowStaticObjectDamage(byCharacter,
                                                           targetStaticWorldObject,
                                                           showClientNotification: false))
            {
                return 0;
            }

            var damage = this.StructureDamage
                         * (1 - structureExplosiveDefenseCoef * (1 - explosiveDefensePenetrationCoef));
            return damage;
        }

        public virtual void ServerExecuteExplosion(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            WeaponFinalCache weaponFinalCache)
        {
            WeaponExplosionSystem.ServerProcessExplosionCircle(
                positionEpicenter: positionEpicenter,
                physicsSpace: physicsSpace,
                damageDistanceMax: this.DamageRadius,
                weaponFinalCache: weaponFinalCache,
                damageOnlyDynamicObjects: false,
                isDamageThroughObstacles: this.IsDamageThroughObstacles,
                callbackCalculateDamageCoefByDistanceForStaticObjects:
                this.ServerCalculateDamageCoefByDistanceForStaticObjects,
                callbackCalculateDamageCoefByDistanceForDynamicObjects: this
                    .ServerCalculateDamageCoefByDistanceForDynamicObjects);
        }

        public virtual void ServerOnObjectHitByExplosion(
            IWorldObject worldObject,
            double damage,
            WeaponFinalCache weaponCache)
        {
        }

        public void ServerSetup(IStaticWorldObject worldObject, ICharacter deployedByCharacter)
        {
            GetPrivateState(worldObject).DeployedByCharacter = deployedByCharacter
                                                               ?? throw new Exception("No character provided");
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            if (IsServer
                && this.IsExplosionDelaySkippedOnDamage)
            {
                // an explosive object was damaged
                var privateState = GetPrivateState(targetObject);
                if (privateState.ExplosionDelaySecondsRemains > 0.1)
                {
                    // explode soon!
                    privateState.ExplosionDelaySecondsRemains = 0.1;
                }
            }

            obstacleBlockDamageCoef = 0;
            damageApplied = 0; // no damage
            return false;      // no hit
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;

            worldObject.ClientSceneObject
                       .AddComponent<ClientComponentBombCountdown>()
                       .Setup(secondsRemains: this.ExplosionDelay.TotalSeconds,
                              positionOffset: this.SharedGetObjectCenterWorldOffset(worldObject)
                                              + (0, 0.55));

            // preload all the explosion spritesheets
            foreach (var textureAtlasResource in this.ExplosionPreset.SpriteAtlasResources)
            {
                Client.Rendering.PreloadTextureAsync(textureAtlasResource);
            }

            ClientGroundExplosionAnimationHelper.PreloadContent();

            // preload all the explosion sounds
            foreach (var soundResource in this.ExplosionPreset.SoundSet)
            {
                Client.Audio.PreloadSoundAsync(soundResource, is3D: true);
            }

            Client.Rendering.PreloadEffectAsync(SharedExplosionHelper.EffectResourceAdditiveColorEffect);
        }

        protected override void ClientOnObjectDestroyed(Vector2D position)
        {
            //base.ClientOnObjectDestroyed(tilePosition);
            //Logger.Important(this + " exploded at " + position);
            SharedExplosionHelper.ClientExplode(position: position + this.Layout.Center,
                                                this.ExplosionPreset,
                                                this.VolumeExplosion);
        }

        protected virtual void ClientSetupExplosionBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            var bounds = this.Layout.Bounds;
            var sizeX = bounds.MaxX - bounds.MinX + 2 * this.DamageRadius - 1;
            var sizeY = bounds.MaxY - bounds.MinY + 2 * this.DamageRadius - 1;
            var ellipse = new Ellipse()
            {
                Width = sizeX * ScriptingConstants.TileSizeVirtualPixels,
                Height = sizeY * ScriptingConstants.TileSizeVirtualPixels,
                Fill = ExplosionBlueprintFillBrush.Value,
                Stroke = ExplosionBlueprintBorderBrush.Value,
                StrokeThickness = 4
            };

            // workaround for NoesisGUI
            ellipse.SetValue(Shape.StrokeDashArrayProperty, "5,2.5");

            Api.Client.UI.AttachControl(
                blueprint.SceneObject,
                positionOffset: this.Layout.Center,
                uiElement: ellipse,
                isFocusable: false,
                isScaleWithCameraZoom: true);
        }

        protected abstract void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution);

        protected abstract void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef);

        protected abstract void PrepareProtoObjectExplosive(out ExplosionPreset explosionPresets);

        protected sealed override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.PrepareProtoObjectExplosive(out var explosionPreset);
            this.ExplosionPreset = explosionPreset
                                   ?? throw new Exception("No explosion preset provided");

            // prepare damage description for characters
            {
                var damageDistribution = new DamageDistribution();
                this.PrepareDamageDescriptionCharacters(
                    out var damageValue,
                    out var armorPiercingCoef,
                    out var finalDamageMultiplier,
                    damageDistribution);

                this.damageDescriptionCharacters = new DamageDescription(
                    damageValue: this.SharedPrepareStatDamageToCharacters(damageValue),
                    armorPiercingCoef,
                    finalDamageMultiplier,
                    rangeMax: this.DamageRadius,
                    damageDistribution);
            }

            // prepare damage properties for structures
            {
                this.PrepareDamageDescriptionStructures(
                    out var damageValue,
                    out var defencePenetrationCoef);

                this.StructureDamage = this.SharedPrepareStatDamageToStructures(damageValue);
                this.StructureDefensePenetrationCoef = MathHelper.Clamp(defencePenetrationCoef, 0, 1);
            }
        }

        protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
        {
            tileRequirements
                .Clear()
                .Add(
                    new ConstructionTileRequirements.Validator(
                        ConstructionTileRequirements.ErrorNoFreeSpace,
                        c =>
                        {
                            var kind = c.ProtoStaticObjectToBuild.Kind;
                            if (kind == StaticObjectKind.Floor
                                || kind == StaticObjectKind.FloorDecal)
                            {
                                return c.Tile.StaticObjects.All(
                                    o => o.ProtoStaticWorldObject.Kind != StaticObjectKind.Floor
                                         && o.ProtoStaticWorldObject.Kind != StaticObjectKind.FloorDecal
                                         && o.ProtoStaticWorldObject.Kind != StaticObjectKind.Platform);
                            }

                            if (c.Tile.StaticObjects.All(
                                    o => o.ProtoStaticWorldObject.Kind == StaticObjectKind.Floor
                                         || o.ProtoStaticWorldObject.Kind == StaticObjectKind.FloorDecal
                                         || o.ProtoStaticWorldObject.Kind == StaticObjectKind.Platform))
                            {
                                // no static objects except floor
                                return true;
                            }

                            // has any static objects in the tile - check whether they are physical obstacles
                            return !ConstructionTileRequirements.TileHasAnyPhysicsObjectsWhere(
                                       c.Tile,
                                       t =>
                                       {
                                           if (!t.PhysicsBody.IsStatic)
                                           {
                                               // allow
                                               return false;
                                           }

                                           switch (t.PhysicsBody.AssociatedWorldObject
                                                    ?.ProtoWorldObject)
                                           {
                                               case IProtoObjectDeposit: // allow deposits
                                               case ObjectWallDestroyed: // allow destroyed walls
                                                   return false;
                                           }

                                           return true;
                                       });
                        }))
                .Add(ConstructionTileRequirements.ValidatorSolidGroundOrPlatform);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            data.PrivateState.ExplosionDelaySecondsRemains = this.ExplosionDelay.TotalSeconds;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var privateState = data.PrivateState;
            privateState.ExplosionDelaySecondsRemains -= data.DeltaTime;
            if (privateState.ExplosionDelaySecondsRemains > 0)
            {
                return;
            }

            this.ServerExplode(data.GameObject, privateState.DeployedByCharacter);
        }

        // this method is made sealed to avoid possible problem with players stuck in the tile of placement
        // see method PrepareTileRequirements
        protected sealed override void SharedCreatePhysics(CreatePhysicsData data)
        {
        }

        protected virtual double SharedPrepareStatDamageToCharacters(double damageValue)
        {
            return damageValue * WeaponConstants.DamageExplosivesToCharactersMultiplier;
        }

        protected virtual double SharedPrepareStatDamageToStructures(double damageValue)
        {
            return damageValue * WeaponConstants.DamageExplosivesToStructuresMultiplier;
        }

        private void ServerExplode(IStaticWorldObject worldObject, ICharacter character)
        {
            try
            {
                Logger.Important(worldObject + " exploded");
                this.ServerSendObjectDestroyedEvent(worldObject);

                SharedExplosionHelper.ServerExplode(
                    character: character,
                    protoExplosive: this,
                    protoWeapon: null,
                    explosionPreset: this.ExplosionPreset,
                    epicenterPosition: worldObject.TilePosition.ToVector2D() + this.Layout.Center,
                    damageDescriptionCharacters: this.damageDescriptionCharacters,
                    physicsSpace: worldObject.PhysicsBody.PhysicsSpace,
                    executeExplosionCallback: this.ServerExecuteExplosion,
                    allowNpcToNpcDamage: this.AllowNpcToNpcDamage);

                if (this.IsActivatesRaidBlock)
                {
                    var explosionRadius = (int)Math.Ceiling(this.DamageRadius);
                    var bounds = new RectangleInt(x: worldObject.TilePosition.X - explosionRadius,
                                                  y: worldObject.TilePosition.Y - explosionRadius,
                                                  width: 2 * explosionRadius,
                                                  height: 2 * explosionRadius);

                    if (RaidingProtectionSystem.SharedCanRaid(bounds,
                                                              showClientNotification: false))
                    {
                        // try activate the raidblock
                        LandClaimSystem.ServerOnRaid(bounds,
                                                     character,
                                                     durationMultiplier: this.RaidBlockDurationMutliplier,
                                                     isStructureDestroyed: false);
                    }
                    else
                    {
                        // Raiding is not possible now due to raiding window
                        // Find if there is any land claim and in that case notify nearby players
                        // that the damage to objects there was not applied.
                        if (LandClaimSystem.SharedIsLandClaimedByAnyone(bounds))
                        {
                            using var tempPlayers = Api.Shared.GetTempList<ICharacter>();
                            Server.World.GetScopedByPlayers(worldObject, tempPlayers);
                            RaidingProtectionSystem.ServerNotifyShowNotificationRaidingNotAvailableNow(
                                tempPlayers.AsList());
                        }
                    }
                }
            }
            finally
            {
                Server.World.DestroyObject(worldObject);
            }
        }
    }

    public abstract class ProtoObjectExplosive
        : ProtoObjectExplosive
            <ObjectExplosivePrivateState,
                StaticObjectPublicState,
                StaticObjectClientState>
    {
    }
}