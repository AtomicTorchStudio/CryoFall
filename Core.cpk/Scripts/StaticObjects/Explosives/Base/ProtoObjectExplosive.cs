namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.ItemExplosive;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
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
        private DamageDescription damageDescriptionCharacters;

        public abstract bool ActivatesRaidModeForLandClaim { get; }

        public abstract double DamageRadius { get; }

        public virtual TimeSpan ExplosionDelay { get; } = TimeSpan.FromSeconds(5);

        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public sealed override double ObstacleBlockDamageCoef => 0f;

        public sealed override double ServerUpdateIntervalSeconds => 0; // every frame

        public double StructureDamage { get; set; }

        public double StructureDefensePenetrationCoef { get; set; }

        public sealed override float StructurePointsMax => 9001; // it's non-damageable anyway

        public virtual float VolumeExplosion => 1;

        protected ExplosionPreset ExplosionPreset { get; private set; }

        public virtual double ServerCalculateDamageCoefByDistance(double distance)
        {
            distance -= 1.42;
            distance = Math.Max(0, distance);
            return 1 - (distance / this.DamageRadius);
        }

        public virtual double ServerCalculateTotalDamageByExplosive(
            IProtoStaticWorldObject targetStaticWorldObjectProto)
        {
            var structureExplosiveDefenseCoef = targetStaticWorldObjectProto.StructureExplosiveDefenseCoef;
            structureExplosiveDefenseCoef = MathHelper.Clamp(structureExplosiveDefenseCoef, 0, 1);

            var explosiveDefensePenetrationCoef = this.StructureDefensePenetrationCoef;
            explosiveDefensePenetrationCoef = MathHelper.Clamp(explosiveDefensePenetrationCoef, 0, 1);
            var damage = this.StructureDamage
                         * (1 - structureExplosiveDefenseCoef * (1 - explosiveDefensePenetrationCoef));
            return damage;
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
            if (IsServer)
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
            return false; // no hit
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;

            Client.Scene.GetSceneObject(worldObject)
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

            Client.Rendering.PreloadEffectAsync(ExplosionHelper.EffectResourceAdditiveColorEffect);
        }

        protected override void ClientOnObjectDestroyed(Vector2Ushort tilePosition)
        {
            //base.ClientOnObjectDestroyed(tilePosition);
            Logger.Important(this + " exploded at " + tilePosition);
            var epicenterPosition = tilePosition.ToVector2D() + this.Layout.Center;
            ExplosionHelper.ClientExplode(epicenterPosition,
                                          this.ExplosionPreset,
                                          this.VolumeExplosion);
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
                    damageValue * WeaponConstants.DamageExplosivesToCharactersMultiplier,
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

                this.StructureDamage = damageValue * WeaponConstants.DamageExplosivesToStructuresMultiplier;
                this.StructureDefensePenetrationCoef = MathHelper.Clamp(defencePenetrationCoef, 0, 1);
            }
        }

        protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
        {
            tileRequirements.Clear()
                            //// do these checks if the bomb will have physics!
                            //.Add(ConstructionTileRequirements.ValidatorClientOnlyNoCurrentPlayer)
                            //.Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyDynamic)
                            .Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyStatic);
        }

        protected virtual void ServerExecuteExplosion(
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
                callbackCalculateDamageCoefByDistance: this.ServerCalculateDamageCoefByDistance);
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

        private void ServerExplode(IStaticWorldObject worldObject, ICharacter character)
        {
            Logger.Important(worldObject + " exploded");

            this.ServerSendObjectDestroyedEvent(worldObject);

            ExplosionHelper.ServerExplode(
                character: character,
                protoObjectExplosive: this,
                explosionPreset: this.ExplosionPreset,
                epicenterPosition: worldObject.TilePosition.ToVector2D() + this.Layout.Center,
                damageDescriptionCharacters: this.damageDescriptionCharacters,
                physicsSpace: worldObject.PhysicsBody.PhysicsSpace,
                executeExplosionCallback: this.ServerExecuteExplosion);

            Server.World.DestroyObject(worldObject);

            if (this.ActivatesRaidModeForLandClaim)
            {
                LandClaimSystem.ServerOnRaid(worldObject.TilePosition, this.DamageRadius, character);
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