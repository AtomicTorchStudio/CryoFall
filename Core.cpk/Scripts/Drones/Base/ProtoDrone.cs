namespace AtomicTorch.CBND.CoreMod.Drones
{
    using System;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDroneControl;
    using AtomicTorch.CBND.CoreMod.Systems.ItemExplosive;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.SoundCue;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoDrone
        <TItemDrone,
         TPrivateState,
         TPublicState,
         TClientState>
        : ProtoDynamicWorldObject
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoDrone
        where TItemDrone : IProtoItemDrone, new()
        where TPrivateState : DronePrivateState, new()
        where TPublicState : DronePublicState, new()
        where TClientState : BaseClientState, new()
    {
        private static readonly double DistanceThresholdToMineral = 0.15;

        private static readonly double DistanceThresholdToPlayer = 0.3;

        private static readonly Lazy<TItemDrone> LazyProtoItemDrone
            = new(Api.GetProtoEntity<TItemDrone>);

        private double lastDroneReturnSoundTime;

        private double lastDroneStartSoundTime;

        protected ProtoDrone()
        {
            var name = this.GetType().Name;
            if (!name.StartsWith("Drone", StringComparison.Ordinal))
            {
                throw new Exception("Drone class name must start with \"Drone\": " + this.GetType().Name);
            }

            this.ShortId = name.Substring("Drone".Length);
            // ReSharper disable once VirtualMemberCallInConstructor
            this.DefaultTextureResource = this.CreateDefaultTexture();
        }

        public virtual Color BeamColor => LightColors.LaserMining;

        public virtual double BeamMaxLength => 0.667;

        public abstract Vector2D BeamOriginOffset { get; }

        public virtual ITextureResource BeamOriginTexture { get; }
            = new TextureResource("FX/Drones/MiningBeamOrigin");

        public virtual double BeamWidth => 1.5;

        public override double ClientUpdateIntervalSeconds => 1;

        public virtual ITextureResource DefaultTextureResource { get; }

        public virtual float DestroyedExplosionVolume => 1;

        public DamageDescription DestroyedVehicleDamageDescriptionCharacters { get; private set; }

        public double DestroyedVehicleDamageRadius { get; private set; }

        public sealed override string Name => LazyProtoItemDrone.Value.Name;

        public abstract double PhysicsBodyAccelerationCoef { get; }

        public abstract double PhysicsBodyFriction { get; }

        public IProtoItemDrone ProtoItemDrone => LazyProtoItemDrone.Value;

        public IProtoItemWeapon ProtoItemMiningTool { get; private set; }

        public override double ServerUpdateIntervalSeconds => 0.1;

        public override string ShortId { get; }

        public abstract double StatMoveSpeed { get; }

        protected virtual ITextureResource BeamTexture { get; }
            = new TextureResource("FX/Drones/MiningBeam.png");

        protected ExplosionPreset DestroyedExplosionPreset { get; private set; }

        protected virtual byte DestroyedExplosionRadius => 30;

        protected abstract double DrawVerticalOffset { get; }

        protected virtual SoundResource DroneReturnOrDropSoundResource { get; }
            = new("Items/Drones/Return");

        protected virtual SoundResource DroneStartSoundResource { get; }
            = new("Items/Drones/Start");

        protected abstract SoundResource EngineSoundResource { get; }

        protected abstract double EngineSoundVolume { get; }

        protected virtual double ShadowOpacity => 0.5;

        public void ServerDropDroneToGround(IDynamicWorldObject objectDrone)
        {
            var privateState = objectDrone.GetPrivateState<DronePrivateState>();
            var storageContainer = privateState.StorageItemsContainer;
            if (storageContainer.OccupiedSlotsCount == 0)
            {
                return;
            }

            // drop all items on the ground
            IItemsContainer groundContainer = null;
            if (privateState.CharacterOwner is not null)
            {
                groundContainer = ObjectPlayerLootContainer.ServerTryCreateLootContainer(privateState.CharacterOwner,
                    objectDrone.Position);

                if (groundContainer is not null)
                {
                    // set slots count matching the total occupied slots count
                    Server.Items.SetSlotsCount(
                        groundContainer,
                        (byte)Math.Min(byte.MaxValue,
                                       groundContainer.OccupiedSlotsCount
                                       + storageContainer.OccupiedSlotsCount));
                }
            }

            groundContainer ??=
                ObjectGroundItemsContainer.ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(
                    privateState.CharacterOwner,
                    objectDrone.Tile);

            if (groundContainer is not null)
            {
                Server.Items.TryMoveAllItems(storageContainer, groundContainer);
                WorldObjectClaimSystem.ServerTryClaim(
                    groundContainer.OwnerAsStaticObject,
                    privateState.CharacterOwner,
                    durationSeconds: ObjectPlayerLootContainer.AutoDestroyTimeoutSeconds
                                     + (10 * 60));
            }
            else
            {
                // TODO: try to create a ground container in any other ground spot
                Logger.Error("Cannot find a place to drop the drone contents on the ground - drone lost!"
                             + objectDrone);
            }
        }

        public IItemsContainer ServerGetStorageItemsContainer(IDynamicWorldObject objectDrone)
        {
            return GetPrivateState(objectDrone).StorageItemsContainer;
        }

        public override void ServerOnDestroy(IDynamicWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);
            CharacterDroneControlSystem.ServerOnDroidDestroyed(gameObject);
        }

        public void ServerOnDroneDroppedOrReturned(
            IDynamicWorldObject objectDrone,
            ICharacter toCharacter,
            bool isReturnedToPlayer)
        {
            if (!isReturnedToPlayer)
            {
                toCharacter = null;
            }

            using var observers = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(objectDrone, observers);

            this.CallClient(observers.AsList(),
                            _ => _.ClientRemote_OnDroneDroppedOrReturned(toCharacter,
                                                                         objectDrone.TilePosition,
                                                                         isReturnedToPlayer));
        }

        public void ServerSetDroneTarget(
            IDynamicWorldObject objectDrone,
            IStaticWorldObject targetWorldObject,
            Vector2D fromStartPosition)
        {
            var targetObjectPosition = targetWorldObject.TilePosition;
            var publicState = GetPublicState(objectDrone);

            var targetDronePosition = targetObjectPosition.ToVector2D()
                                      + DroneTargetPositionHelper.GetTargetPosition(targetWorldObject);

            // ensure the drone will select a position on the side of the object
            var directionX = fromStartPosition.X - targetDronePosition.X;
            if (Math.Abs(directionX) < 0.5)
            {
                directionX = 0.5 * Math.Sign(directionX);
            }
            else if (Math.Abs(directionX) > 1.0)
            {
                directionX = 1.0 * Math.Sign(directionX);
            }

            if (targetWorldObject.ProtoGameObject is IProtoObjectTree)
            {
                // ensure the drone will always fly somewhere above below the tree
                // (a beam behind the tree doesn't look good)
                targetDronePosition -= (0, 0.5);
                directionX = Math.Sign(directionX) * 1.5;
            }
            else
            {
                // ensure the drone will always fly somewhere above the object
                targetDronePosition += (0, 0.5);
            }

            targetDronePosition += new Vector2D(directionX * this.BeamMaxLength, 0);

            publicState.SetTargetPosition(targetObjectPosition, targetDronePosition);

            CharacterDroneControlSystem.ServerUnregisterCurrentMining(objectDrone);
        }

        public void ServerSetupAssociatedItem(IDynamicWorldObject objectDrone, IItem item)
        {
            GetPrivateState(objectDrone).AssociatedItem = item;
        }

        public void ServerStartDrone(
            IDynamicWorldObject objectDrone,
            ICharacter character,
            bool isFromHotbarContainer,
            byte fromSlotIndex)
        {
            var privateState = GetPrivateState(objectDrone);
            if (!privateState.IsDespawned)
            {
                Logger.Error("The drone is already spawned: " + objectDrone, character);
                return;
            }

            privateState.CharacterOwner = character;
            privateState.IsDespawned = false;
            objectDrone.ProtoGameObject.ServerSetUpdateRate(objectDrone, isRare: false);
            privateState.IsStartedFromHotbarContainer = isFromHotbarContainer;
            privateState.StartedFromSlotIndex = fromSlotIndex;
            Server.World.SetPosition(objectDrone, character.Position, writeToLog: false);
            // recreate physics (as spawned drone has physics)
            objectDrone.ProtoWorldObject.SharedCreatePhysics(objectDrone);

            using var observers = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(objectDrone, observers);
            this.CallClient(observers.AsList(),
                            _ => _.ClientRemote_OnDroneStart(objectDrone, character.Id));
        }

        protected virtual IComponentSpriteRenderer ClientCreateRendererShadow(
            IDynamicWorldObject worldObject,
            double scaleMultiplier)
        {
            var rendererShadow = ClientShadowHelper.AddShadowRenderer(
                worldObject,
                scaleMultiplier: (float)scaleMultiplier);

            if (rendererShadow is null)
            {
                return null;
            }

            rendererShadow.PositionOffset = default;
            rendererShadow.Color = Color.FromArgb((byte)(byte.MaxValue * this.ShadowOpacity), 0x00, 0x00, 0x00);
            rendererShadow.DrawOrder = DrawOrder.Shadow;
            return rendererShadow;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var objectDrone = data.GameObject;

            var spriteRenderer = this.ClientSetupRendering(data);
            this.ClientCreateRendererShadow(data.GameObject, scaleMultiplier: 1.0);
            this.ClientSetupSoundEmitter(data);
            this.ClientSetupHealthbar(data);

            var sceneObject = objectDrone.ClientSceneObject;
            sceneObject.AddComponent<ComponentDroneVisualManager>()
                       .Setup(objectDrone,
                              spriteRenderer,
                              this.StatMoveSpeed);

            sceneObject.AddComponent<ComponentDroneMiningBeam>()
                       .Setup(data.PublicState,
                              beamColor: this.BeamColor,
                              beamOriginTexture: this.BeamOriginTexture,
                              beamTexture: this.BeamTexture,
                              beamOriginOffset: this.BeamOriginOffset,
                              beamWidth: this.BeamWidth,
                              primaryRenderer: spriteRenderer);
        }

        protected virtual IComponentSpriteRenderer ClientSetupRendering(ClientInitializeData data)
        {
            var worldObject = data.GameObject;
            var spriteRenderer = Client.Rendering.CreateSpriteRenderer(
                worldObject,
                this.DefaultTextureResource,
                spritePivotPoint: (0.5, 0.5));
            //spriteRenderer.DrawOrderOffsetY = -1;
            spriteRenderer.Scale = 0.75;
            spriteRenderer.PositionOffset = (0, this.DrawVerticalOffset);
            return spriteRenderer;
        }

        protected virtual IComponentSoundEmitter ClientSetupSoundEmitter(ClientInitializeData data)
        {
            return Client.Audio.CreateSoundEmitter(
                data.GameObject,
                this.EngineSoundResource,
                is3D: true,
                isLooped: true,
                radius: this.ObjectSoundRadius,
                volume: (float)this.EngineSoundVolume,
                isPlaying: true);
        }

        protected virtual ITextureResource CreateDefaultTexture()
        {
            return new TextureResource("Drones/" + this.GetType().Name);
        }

        protected abstract void PrepareProtoDrone(out IProtoItemWeapon protoTool);

        protected sealed override void PrepareProtoDynamicWorldObject()
        {
            base.PrepareProtoDynamicWorldObject();

            this.PrepareProtoDrone(out var protoTool);
            this.ProtoItemMiningTool = protoTool;

            this.PrepareProtoVehicleDestroyedExplosionPreset(
                out var destroyedVehicleDamageRadius,
                out var destroyedExplosionPreset,
                out var destroyedVehicleDamageDescriptionCharacters);

            this.DestroyedExplosionPreset = destroyedExplosionPreset
                                            ?? throw new Exception("No explosion preset provided");

            this.DestroyedVehicleDamageRadius = destroyedVehicleDamageRadius;
            this.DestroyedVehicleDamageDescriptionCharacters = destroyedVehicleDamageDescriptionCharacters;
        }

        protected abstract void PrepareProtoVehicleDestroyedExplosionPreset(
            out double damageRadius,
            out ExplosionPreset explosionPreset,
            out DamageDescription damageDescriptionCharacters);

        protected virtual void ServerExecuteVehicleExplosion(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            WeaponFinalCache weaponFinalCache)
        {
            WeaponExplosionSystem.ServerProcessExplosionCircle(
                positionEpicenter: positionEpicenter,
                physicsSpace: physicsSpace,
                damageDistanceMax: this.DestroyedVehicleDamageRadius,
                weaponFinalCache: weaponFinalCache,
                damageOnlyDynamicObjects: false,
                isDamageThroughObstacles: false,
                callbackCalculateDamageCoefByDistanceForStaticObjects: CalcDamageCoefByDistance,
                callbackCalculateDamageCoefByDistanceForDynamicObjects: CalcDamageCoefByDistance);

            double CalcDamageCoefByDistance(double distance)
            {
                var distanceThreshold = 0.5;
                if (distance <= distanceThreshold)
                {
                    return 1;
                }

                distance -= distanceThreshold;
                distance = Math.Max(0, distance);

                var maxDistance = this.DestroyedVehicleDamageRadius;
                maxDistance -= distanceThreshold;
                maxDistance = Math.Max(0, maxDistance);

                return 1 - Math.Min(distance / maxDistance, 1);
            }
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            var objectDrone = data.GameObject;
            base.ServerInitialize(data);

            if (data.IsFirstTimeInit)
            {
                data.PrivateState.StorageItemsContainer
                    = Api.Server.Items.CreateContainer<ItemsContainerDefault>(objectDrone,
                                                                              slotsCount: 255);
            }
        }

        protected override void ServerOnDynamicObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            var vehicle = (IDynamicWorldObject)targetObject;

            // explode
            using var scopedBy = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetCharactersInRadius(vehicle.TilePosition,
                                               scopedBy,
                                               radius: this.DestroyedExplosionRadius,
                                               onlyPlayers: true);

            this.CallClient(scopedBy.AsList(),
                            _ => _.ClientRemote_VehicleExploded(vehicle.Position));

            SharedExplosionHelper.ServerExplode(
                character:
                null, // yes, no damaging character here otherwise it will not receive the damage if staying close
                protoExplosive: null,
                protoWeapon: null,
                explosionPreset: this.DestroyedExplosionPreset,
                epicenterPosition: vehicle.Position
                                   + this.SharedGetObjectCenterWorldOffset(targetObject),
                damageDescriptionCharacters: this.DestroyedVehicleDamageDescriptionCharacters,
                physicsSpace: targetObject.PhysicsBody.PhysicsSpace,
                executeExplosionCallback: this.ServerExecuteVehicleExplosion);

            // destroy the vehicle completely after short explosion delay
            ServerTimersSystem.AddAction(
                this.DestroyedExplosionPreset.ServerDamageApplyDelay,
                () => base.ServerOnDynamicObjectZeroStructurePoints(weaponCache,
                                                                    byCharacter,
                                                                    targetObject));
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var objectDrone = data.GameObject;
            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            if (privateState.AssociatedItem is null)
            {
                // incorrectly configured drone
                Server.World.DestroyObject(objectDrone);
                return;
            }

            if (privateState.IsDespawned)
            {
                this.ServerSetUpdateRate(objectDrone, isRare: true);
                return;
            }

            UpdateWeaponCooldown();

            Vector2D destinationCoordinate;
            var characterOwner = privateState.CharacterOwner;

            var hasMiningTargets = publicState.TargetObjectPosition.HasValue;

            if (hasMiningTargets
                && !(CharacterDroneControlSystem.SharedIsValidDroneOperationDistance(objectDrone.TilePosition,
                         characterOwner.TilePosition)
                     && objectDrone.Tile.Height == characterOwner.Tile.Height))
            {
                Logger.Info("The drone is beyond operation distance or has different tile height and will be recalled: "
                            + objectDrone);
                CharacterDroneControlSystem.ServerRecallDrone(objectDrone);
                hasMiningTargets = false;
            }

            if (hasMiningTargets)
            {
                // go to the next waypoint
                destinationCoordinate = publicState.TargetDronePosition
                                        ?? publicState.TargetObjectPosition.Value.ToVector2D();

                if (!CharacterDroneControlSystem.ServerIsMiningAllowed(
                        publicState.TargetObjectPosition.Value,
                        objectDrone))
                {
                    // cannot mine as it's already mined by another drone
                    publicState.ResetTargetPosition();
                    return;
                }
            }
            else
            {
                // should return to the player to despawn
                if (characterOwner is null
                    || characterOwner.GetPublicState<ICharacterPublicState>().IsDead
                    || CharacterDroneControlSystem.SharedIsBeyondDroneAbandonDistance(
                        objectDrone.TilePosition,
                        characterOwner.TilePosition))
                {
                    CharacterDroneControlSystem.ServerDeactivateDrone(objectDrone);
                    return;
                }

                destinationCoordinate = characterOwner.Position;
            }

            RefreshMovement(isToMineral: hasMiningTargets,
                            destinationCoordinate,
                            out var isDestinationReached);
            if (!isDestinationReached)
            {
                return;
            }

            if (hasMiningTargets)
            {
                PerformMining();
            }
            else
            {
                // we were going to the player and reached its location, despawn
                ServerOnDroneReturnedToPlayer(objectDrone);
            }

            void RefreshMovement(
                bool isToMineral,
                Vector2D toPosition,
                out bool isDestinationReached)
            {
                var positionDelta = toPosition - objectDrone.Position;
                var positionDeltaLength = positionDelta.Length;

                double targetVelocity;
                if (positionDeltaLength
                    > (isToMineral
                           ? DistanceThresholdToMineral
                           : DistanceThresholdToPlayer))
                {
                    // fly towards that object
                    var moveSpeed = this.StatMoveSpeed;

                    targetVelocity = moveSpeed;
                    isDestinationReached = false;

                    if (isToMineral)
                    {
                        // reduce speed when too close to the mineral
                        var distanceCoef = positionDeltaLength / (0.333 * targetVelocity);
                        if (distanceCoef < 1)
                        {
                            targetVelocity *= Math.Pow(distanceCoef, 0.5);
                            // ensure it cannot drop lower than 5% of the original move speed
                            targetVelocity = Math.Max(0.05 * moveSpeed, targetVelocity);
                        }
                    }
                }
                else
                {
                    isDestinationReached = true;

                    // stop
                    if (Server.World.GetDynamicObjectMoveSpeed(objectDrone) == 0)
                    {
                        // already stopped
                        return;
                    }

                    targetVelocity = 0;
                }

                var movementDirectionNormalized = positionDelta.Normalized;
                var moveAcceleration = movementDirectionNormalized * this.PhysicsBodyAccelerationCoef * targetVelocity;

                Server.World.SetDynamicObjectMoveSpeed(objectDrone, targetVelocity);

                Server.World.SetDynamicObjectPhysicsMovement(objectDrone,
                                                             moveAcceleration,
                                                             targetVelocity: targetVelocity);
                objectDrone.PhysicsBody.Friction = this.PhysicsBodyFriction;
            }

            void PerformMining()
            {
                var targetObject = CharacterDroneControlSystem.SharedGetCompatibleTarget(
                    characterOwner,
                    publicState.TargetObjectPosition.Value,
                    out _,
                    out _);
                if (targetObject is null
                    || !WorldObjectClaimSystem.SharedIsAllowInteraction(characterOwner,
                                                                        targetObject,
                                                                        showClientNotification: false))
                {
                    // nothing to mine there, or finished mining, or cannot mine
                    CharacterDroneControlSystem.ServerUnregisterCurrentMining(
                        publicState.TargetObjectPosition.Value,
                        objectDrone);

                    publicState.ResetTargetPosition();
                    return;
                }

                if (privateState.WeaponCooldownSecondsRemains > 0)
                {
                    return;
                }

                if (!CharacterDroneControlSystem.ServerTryRegisterCurrentMining(
                        publicState.TargetObjectPosition.Value,
                        objectDrone))
                {
                    // cannot mine as it's already mined by another drone
                    publicState.ResetTargetPosition();
                    return;
                }

                publicState.IsMining = true;

                var protoMiningTool = this.ProtoItemMiningTool;
                privateState.WeaponCooldownSecondsRemains +=
                    Api.Shared.RoundDurationByServerFrameDuration(protoMiningTool.FireInterval);

                var characterFinalStatsCache = characterOwner.SharedGetFinalStatsCache();

                var weaponFinalCache = privateState.WeaponFinalCache;
                if (weaponFinalCache is null
                    || !privateState.LastCharacterOwnerFinalStatsCache.CustomEquals(characterFinalStatsCache))
                {
                    weaponFinalCache = ServerCreateWeaponFinalCacheForDrone(characterOwner,
                                                                            protoMiningTool,
                                                                            objectDrone);

                    privateState.WeaponFinalCache = weaponFinalCache;
                    privateState.LastCharacterOwnerFinalStatsCache = characterFinalStatsCache;
                }

                var protoWorldObject = (IDamageableProtoWorldObject)targetObject.ProtoGameObject;
                protoWorldObject.SharedOnDamage(
                    weaponFinalCache,
                    targetObject,
                    damagePreMultiplier: 1,
                    damagePostMultiplier: 1,
                    obstacleBlockDamageCoef: out _,
                    damageApplied: out var damageApplied);

                // reduce drone durability on 1 unit (reflected as HP when it's a world object)
                // but ensure the new HP cannot drop to exact 0 (to prevent destruction while mining)
                var newHP = publicState.StructurePointsCurrent
                            - 1 * LazyProtoItemDrone.Value.DurabilityToStructurePointsConversionCoefficient;
                publicState.StructurePointsCurrent = Math.Max(float.Epsilon, (float)newHP);

                if (damageApplied <= 0)
                {
                    // cannot mine there for whatever reason, recall the drone
                    publicState.ResetTargetPosition();
                }

                this.ServerSendMiningSoundCue(objectDrone, characterOwner);
            }

            void UpdateWeaponCooldown()
            {
                if (privateState.WeaponCooldownSecondsRemains <= 0)
                {
                    return;
                }

                // process weapon cooldown
                var deltaTime = data.DeltaTime;
                if (deltaTime > 0.4)
                {
                    // too large delta time probably due to a frame skip
                    deltaTime = 0.4;
                }

                if (privateState.WeaponCooldownSecondsRemains > 0)
                {
                    privateState.WeaponCooldownSecondsRemains -= deltaTime;
                    if (privateState.WeaponCooldownSecondsRemains < -0.2)
                    {
                        // clamp the remaining cooldown in case of a frame skip
                        privateState.WeaponCooldownSecondsRemains = -0.2;
                    }
                }
            }
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IDynamicWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            if (PveSystem.SharedIsPve(false)
                && (weaponCache.Character is null
                    || !weaponCache.Character.IsNpc))
            {
                // no PvP damage in PvE (only creature damage is allowed)
                obstacleBlockDamageCoef = 0;
                return 0;
            }

            return base.SharedCalculateDamageByWeapon(weaponCache,
                                                      damagePreMultiplier,
                                                      targetObject,
                                                      out obstacleBlockDamageCoef);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            if (IsServer
                && data.PrivateState.IsDespawned)
            {
                // no physics for despawned drones
                return;
            }

            this.SharedCreatePhysicsDrone(data);
        }

        protected abstract void SharedCreatePhysicsDrone(CreatePhysicsData data);

        private static WeaponFinalCache ServerCreateWeaponFinalCacheForDrone(
            ICharacter characterOwner,
            IProtoItemWeapon protoMiningTool,
            IDynamicWorldObject objectDrone)
        {
            using var tempStatsCache = TempStatsCache.GetFromPool();

            // fill only the skills cache from character (status effects have no effect)
            foreach (var pair in characterOwner.SharedGetSkills().Skills)
            {
                var protoSkill = pair.Key;
                var skillLevel = pair.Value.Level;
                protoSkill.FillEffectsCache(tempStatsCache, skillLevel);
            }

            var finalStatsCache = tempStatsCache.CalculateFinalStatsCache();
            return new WeaponFinalCache(
                characterOwner,
                finalStatsCache,
                weapon: null,
                protoWeapon: protoMiningTool,
                protoAmmo: null,
                damageDescription: protoMiningTool.OverrideDamageDescription,
                protoExplosive: null,
                objectDrone: objectDrone);
        }

        private static void ServerOnDroneReturnedToPlayer(IDynamicWorldObject worldObject)
        {
            var privateState = GetPrivateState(worldObject);
            var character = privateState.CharacterOwner;

            CharacterDroneControlSystem.ServerDespawnDrone(worldObject, isReturnedToPlayer: true);

            var storageContainer = privateState.StorageItemsContainer;
            if (storageContainer.OccupiedSlotsCount == 0)
            {
                return;
            }

            // drop storage container contents to player
            // but first, move the drone item to its original slot (if possible)
            var characterContainerInventory = character.SharedGetPlayerContainerInventory();
            var characterContainerHotbar = character.SharedGetPlayerContainerHotbar();

            var itemInFirstSlot = storageContainer.GetItemAtSlot(0);
            if (itemInFirstSlot is not null)
            {
                // item in the first slot is the drone's associated item
                // it could be destroyed in case the drone's HP dropped <= 1
                Server.Items.MoveOrSwapItem(itemInFirstSlot,
                                            privateState.IsStartedFromHotbarContainer
                                                ? characterContainerHotbar
                                                : characterContainerInventory,
                                            slotId: privateState.StartedFromSlotIndex,
                                            movedCount: out _);
            }

            var result = Server.Items.TryMoveAllItems(storageContainer, characterContainerInventory);
            try
            {
                if (storageContainer.OccupiedSlotsCount == 0)
                {
                    // all items moved from drone to player
                    return;
                }

                // try move remaining items to hotbar
                var resultToHotbar = Server.Items.TryMoveAllItems(storageContainer, characterContainerHotbar);
                result.MergeWith(resultToHotbar,
                                 areAllItemsMoved: resultToHotbar.AreAllItemMoved);

                if (storageContainer.OccupiedSlotsCount == 0)
                {
                    // all items moved from drone to player
                    return;
                }
            }
            finally
            {
                if (result.MovedItems.Count > 0)
                {
                    // notify player about the received items
                    NotificationSystem.ServerSendItemsNotification(
                        character,
                        result.MovedItems
                              .GroupBy(p => p.Key.ProtoItem)
                              .Where(p => !(p.Key is TItemDrone))
                              .ToDictionary(p => p.Key, p => p.Sum(v => v.Value)));
                }
            }

            // try to drop the remaining items on the ground
            var groundContainer = ObjectGroundItemsContainer
                .ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(character, character.Tile);
            if (groundContainer is not null)
            {
                var result2 = Server.Items.TryMoveAllItems(storageContainer, groundContainer);
                if (result2.MovedItems.Count > 0)
                {
                    var protoItemForIcon = result2.MovedItems.First().Key.ProtoItem;

                    NotificationSystem.ServerSendNotificationNoSpaceInInventoryItemsDroppedToGround(
                        character,
                        protoItemForIcon);

                    // ensure that these items could be lifted only by their owner in PvE
                    WorldObjectClaimSystem.ServerTryClaim(groundContainer.OwnerAsStaticObject,
                                                          character,
                                                          WorldObjectClaimDuration.DroppedGoods);
                }
            }

            if (storageContainer.OccupiedSlotsCount == 0)
            {
                return;
            }

            Logger.Error("Not all items dropped on the ground from the drone storage: "
                         + worldObject
                         + " slots occupied: "
                         + storageContainer.OccupiedSlotsCount);
        }

        private void ClientRemote_OnDroneDroppedOrReturned(
            ICharacter toCharacter,
            Vector2Ushort objectDroneTilePosition,
            bool isReturnedToPlayer)
        {
            if (toCharacter is not null
                && !toCharacter.IsInitialized)
            {
                toCharacter = null;
            }

            var time = Client.Core.ClientRealTime;
            if (time - this.lastDroneReturnSoundTime < 0.1)
            {
                //Logger.Dev("Drone return sound throttled");
                return;
            }

            this.lastDroneReturnSoundTime = time;
            //Logger.Dev("Drone return sound");

            if (toCharacter is not null)
            {
                Client.Audio.PlayOneShot(this.DroneReturnOrDropSoundResource, toCharacter);
            }
            else
            {
                Client.Audio.PlayOneShot(this.DroneReturnOrDropSoundResource,
                                         worldPosition: objectDroneTilePosition.ToVector2D() + (0.5, 0.5));
            }
        }

        private void ClientRemote_OnDroneStart(IDynamicWorldObject objectDrone, uint ownerCharacterId)
        {
            if (!objectDrone.IsInitialized)
            {
                return;
            }

            var time = Client.Core.ClientRealTime;
            if (time - this.lastDroneStartSoundTime < 0.1)
            {
                //Logger.Dev("Drone start sound throttled");
                return;
            }

            this.lastDroneStartSoundTime = time;
            //Logger.Dev("Drone start sound at: " + objectDrone.Position);

            if (ownerCharacterId == ClientCurrentCharacterHelper.Character.Id)
            {
                Client.Audio.PlayOneShot(this.DroneStartSoundResource);
            }
            else
            {
                Client.Audio.PlayOneShot(this.DroneStartSoundResource, objectDrone);
            }
        }

        private void ClientRemote_OnMiningSoundCue(
            IDynamicWorldObject objectDrone,
            uint ownerCharacterPartyId,
            Vector2Ushort fallbackPosition)
        {
            if (objectDrone is not null
                && !objectDrone.IsInitialized)
            {
                objectDrone = null;
            }

            var position = objectDrone?.Position ?? fallbackPosition.ToVector2D();
            position += this.BeamOriginOffset;

            var isByPartyMember = ownerCharacterPartyId > 0
                                  && ownerCharacterPartyId == PartySystem.ClientCurrentParty?.Id;
            ClientSoundCueManager.OnSoundEvent(position,
                                               isPartyMember: isByPartyMember);
        }

        private void ClientRemote_VehicleExploded(Vector2D position)
        {
            Logger.Important(this + " exploded at " + position);
            SharedExplosionHelper.ClientExplode(position: position + this.SharedGetObjectCenterWorldOffset(null),
                                          this.DestroyedExplosionPreset,
                                          this.DestroyedExplosionVolume);
        }

        private void ClientSetupHealthbar(ClientInitializeData data)
        {
            var objectDrone = data.GameObject;
            var publicState = data.PublicState;

            var structurePointsBarControl = new VehicleArmorBarControl();
            structurePointsBarControl.Setup(
                new ObjectStructurePointsData(objectDrone, this.SharedGetStructurePointsMax(objectDrone)),
                publicState.StructurePointsCurrent);

            Api.Client.UI.AttachControl(
                objectDrone,
                structurePointsBarControl,
                positionOffset: this.SharedGetObjectCenterWorldOffset(objectDrone) + (0, 0.55),
                isFocusable: false);
        }

        private void ServerSendMiningSoundCue(IDynamicWorldObject objectDrone, ICharacter characterOwner)
        {
            using var observers = Api.Shared.GetTempList<ICharacter>();
            var eventNetworkRadius = (byte)Math.Max(
                15,
                Math.Ceiling(this.ProtoItemMiningTool.SoundPresetWeaponDistance.max));

            Server.World.GetCharactersInRadius(objectDrone.TilePosition,
                                               observers,
                                               radius: eventNetworkRadius,
                                               onlyPlayers: true);
            observers.Remove(characterOwner);

            if (observers.Count == 0)
            {
                return;
            }

            var partyId = PartySystem.ServerGetParty(characterOwner)?.Id ?? 0;

            this.CallClient(observers.AsList(),
                            _ => _.ClientRemote_OnMiningSoundCue(objectDrone,
                                                                 partyId,
                                                                 objectDrone.TilePosition));
        }
    }

    public abstract class ProtoDrone<TItemDrone>
        : ProtoDrone
            <TItemDrone,
                DronePrivateState,
                DronePublicState,
                EmptyClientState>
        where TItemDrone : IProtoItemDrone, new()
    {
    }
}