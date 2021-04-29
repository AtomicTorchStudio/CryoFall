namespace AtomicTorch.CBND.CoreMod.Characters.Turrets
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Helpers;
    using JetBrains.Annotations;

    public abstract class ProtoCharacterTurret
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoCharacter
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoCharacterTurret
        where TPrivateState : CharacterTurretPrivateState, new()
        where TPublicState : CharacterMobPublicState, new()
        where TClientState : BaseCharacterClientState, new()
    {
        private ProtoItemWeaponTurret protoItemWeaponTurret;

        private ProtoCharacterSkeleton protoSkeleton;

        private double protoSkeletonScale;

        public abstract double BarrelRotationRate { get; }

        public override float CharacterWorldHeight => 1.4f;

        public override float CharacterWorldWeaponOffsetRanged => 0.7f;

        public virtual ITextureResource Icon
        {
            get
            {
                this.SharedGetSkeletonProto(null, out var skeletonProto, out _);
                return ((ProtoSkeletonTurret)skeletonProto).Icon;
            }
        }

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal; // not used

        public override double PhysicsBodyAccelerationCoef => 0;

        public override double PhysicsBodyFriction => 0;

        public ProtoItemWeaponTurret ProtoItemWeaponTurret => this.protoItemWeaponTurret;

        public override double ServerUpdateIntervalSeconds => 0.1;

        public override double StatDefaultHealthMax => 9001; // not used

        public override double StatHealthRegenerationPerSecond => 0;

        public override double StatMoveSpeed => 0;

        public override IEnumerable<IItemsContainer> SharedEnumerateAllContainers(
            ICharacter character,
            bool includeEquipmentContainer)
        {
            yield break;
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            double damagePostMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            if (IsClient)
            {
                damageApplied = 0;
                obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
                return true;
            }

            if (weaponCache.ProtoExplosive is not null)
            {
                // no damage by explosives as they're damaging the turret structure directly
                damageApplied = 0;
                obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
                return true;
            }

            // the damage is propagating directly to the linked turret structure
            var objectTurret = GetPrivateState((ICharacter)targetObject).ObjectTurret;
            if (objectTurret is null)
            {
                Logger.Error("No linked turret: " + targetObject);
                obstacleBlockDamageCoef = 0;
                damageApplied = 0;
                return false;
            }

            return ((IProtoObjectStructure)objectTurret.ProtoGameObject)
                .SharedOnDamage(weaponCache,
                                objectTurret,
                                damagePreMultiplier,
                                damagePostMultiplier,
                                out obstacleBlockDamageCoef,
                                out damageApplied);
        }

        public override void SharedRefreshFinalCacheIfNecessary(ICharacter character)
        {
            if (IsServer)
            {
                this.ServerRebuildFinalCacheIfNeeded(GetPrivateState(character),
                                                     GetPublicState(character));
            }
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            ClientCharacterEquipmentHelper.ClientRebuildAppearance(
                data.GameObject,
                data.ClientState,
                data.PublicState,
                data.PublicState.SelectedItem);
        }

        protected sealed override void PrepareProtoCharacter()
        {
            base.PrepareProtoCharacter();

            this.protoSkeletonScale = 1.0;
            this.PrepareProtoCharacterTurret(out this.protoItemWeaponTurret,
                                             out this.protoSkeleton,
                                             ref this.protoSkeletonScale);

            if (this.protoSkeleton is null
                || this.protoSkeletonScale <= 0)
            {
                throw new Exception("Incorrect skeleton or skeleton scale provided");
            }
        }

        protected abstract void PrepareProtoCharacterTurret(
            out ProtoItemWeaponTurret protoItemWeaponTurret,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double protoSkeletonScale);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void ServerForceBuildFinalStatsCache(
            TPrivateState privateState,
            TPublicState publicState)
        {
            SharedCharacterStatsHelper.RefreshCharacterFinalStatsCache(
                this.ProtoCharacterDefaultEffects,
                publicState,
                privateState);
        }

        protected override void ServerInitializeCharacter(ServerInitializeData data)
        {
            // don't call the base initialize
            // base.ServerInitializeCharacter(data);

            var character = data.GameObject;
            var privateState = data.PrivateState;

            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(this.protoItemWeaponTurret);
            data.PublicState.SharedSetCurrentWeaponProtoOnly(this.protoItemWeaponTurret);

            this.ServerInitializeCharacterTurret(data);

            SharedCharacterStatsHelper.RefreshCharacterFinalStatsCache(
                this.ProtoCharacterDefaultEffects,
                data.PublicState,
                privateState,
                // always rebuild the mob character state
                // as the game doesn't store current HP for mobs
                isFirstTime: true);

            var weaponState = privateState.WeaponState;
            WeaponSystem.SharedRebuildWeaponCache(character, weaponState);
            privateState.AttackRange = weaponState.WeaponCache?.RangeMax ?? 0;

            if (data.IsFirstTimeInit)
            {
                privateState.SpawnPosition = character.TilePosition;
            }
        }

        protected virtual void ServerInitializeCharacterTurret(ServerInitializeData data)
        {
        }

        protected sealed override void ServerUpdate(ServerUpdateData data)
        {
            var character = data.GameObject;
            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            var objectTurret = privateState.ObjectTurret;
            if (objectTurret is null)
            {
                return;
            }

            var objectTurretPrivateState = objectTurret.GetPrivateState<ObjectTurretPrivateState>();
            var turretMode = objectTurretPrivateState.TurretMode;
            if (turretMode == TurretMode.Disabled)
            {
                return;
            }

            var objectTurretPublicState = objectTurret.GetPublicState<ObjectTurretPublicState>();
            if (objectTurretPublicState.ElectricityConsumerState != ElectricityConsumerState.PowerOnActive)
            {
                return;
            }

            if (LandClaimShieldProtectionSystem.SharedIsUnderShieldProtection(objectTurret))
            {
                return;
            }

            this.ServerRebuildFinalCacheIfNeeded(privateState, publicState);

            ServerUpdateAggroState(privateState, data.DeltaTime);
            this.ServerUpdateTurret(data, turretMode);

            // update weapon state (fires the weapon if needed)
            WeaponSystem.SharedUpdateCurrentWeapon(
                character,
                privateState.WeaponState,
                data.DeltaTime);
        }

        protected virtual void ServerUpdateTurret(ServerUpdateData data, TurretMode turretMode)
        {
            var character = data.GameObject;
            var privateState = data.PrivateState;
            var publicState = GetPublicState(character);
            var weaponState = privateState.WeaponState;

            if (privateState.WeaponState.ProtoWeapon is ProtoItemWeaponTurretWithAmmo protoItemWeaponTurretWithAmmo)
            {
                protoItemWeaponTurretWithAmmo.ServerUpdateCurrentAmmo(character, weaponState);
                // attack range may change if the ammo type has changed
                privateState.AttackRange = weaponState.WeaponCache?.RangeMax ?? 0;
            }

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                targetCharacter: ServerTurretAiHelper.GetClosestTargetPlayer(character, turretMode, privateState),
                isRetreating: false,
                isRetreatingForHeavyVehicles: false,
                distanceRetreat: 0,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 8,
                movementDirection: out _,
                rotationAngleRad: out var rotationAngleRad,
                attackFarOnlyIfAggro: true,
                customIsValidTargetCallback: IsValidTargetCallback);

            var currentRotationAngleRad = (double)publicState.AppliedInput.RotationAngleRad;
            rotationAngleRad = MathHelper.LerpAngle(currentRotationAngleRad,
                                                    rotationAngleRad,
                                                    data.DeltaTime,
                                                    this.BarrelRotationRate);

            publicState.AppliedInput.Set(
                new CharacterInput()
                {
                    MoveModes = CharacterMoveModes.None,
                    RotationAngleRad = (float)rotationAngleRad
                },
                moveSpeed: 0);

            if (weaponState.SharedGetInputIsFiring()
                && !ServerCharacterAiHelper.CanHitAnyTargetWithRangedWeapon(
                    character,
                    rotationAngleRad,
                    privateState,
                    isValidTargetCallback: IsValidTargetForceAttackNeutralsCallback))
            {
                // don't shoot as not pointing on the target right now 
                weaponState.SharedSetInputIsFiring(false);
            }

            bool IsValidTargetCallback(IWorldObject worldObject)
                => ServerTurretAiHelper.IsValidTarget(character,
                                                      worldObject,
                                                      turretMode,
                                                      forceAttackNeutrals: false);

            bool IsValidTargetForceAttackNeutralsCallback(IWorldObject worldObject)
                => ServerTurretAiHelper.IsValidTarget(character,
                                                      worldObject,
                                                      turretMode,
                                                      forceAttackNeutrals: true);
        }

        protected sealed override void SharedGetSkeletonProto(
            [CanBeNull] ICharacter character,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale)
        {
            protoSkeleton = this.protoSkeleton;
            scale = this.protoSkeletonScale;
        }

        private static void ServerUpdateAggroState(TPrivateState privateState, double deltaTime)
        {
            if (privateState.CurrentAggroTimeRemains <= 0)
            {
                return;
            }

            var timeRemains = privateState.CurrentAggroTimeRemains - deltaTime;
            if (timeRemains <= 0)
            {
                // reset aggro state
                timeRemains = 0;
                privateState.CurrentAggroCharacter = null;
                //Logger.Dev("Aggro state reset: " + privateState.GameObject);
            }

            privateState.CurrentAggroTimeRemains = timeRemains;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ServerRebuildFinalCacheIfNeeded(
            TPrivateState privateState,
            TPublicState publicState)
        {
            if (!privateState.FinalStatsCache.IsDirty)
            {
                return;
            }

            // rebuild stats cache
            this.ServerForceBuildFinalStatsCache(privateState, publicState);
        }
    }

    public abstract class ProtoCharacterTurret
        : ProtoCharacterTurret<
            CharacterTurretPrivateState,
            CharacterMobPublicState,
            CharacterMobClientState>
    {
    }
}