namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.SoundCue;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public abstract class ProtoCharacterMob
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoCharacter
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoCharacterMob
        where TPrivateState : CharacterMobPrivateState, new()
        where TPublicState : CharacterMobPublicState, new()
        where TClientState : BaseCharacterClientState, new()
    {
        public const double AgroStateDuration = 5 * 60; // 5 minutes

        // If the mob is too far away from the spawn position, it should be despawned after the delay.
        private const int DespawnTileDistanceThreshold = 10; // 10+ tiles away

        // If the mob is too far away from the spawn position it will be despawned after this delay (if not observed).
        private const int DespawnTimeThreshold = 10 * 60; // 10 minutes

        private static readonly IWorldServerService ServerWorld = IsServer
                                                                      ? Server.World
                                                                      : null;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly List<ICharacter> TempListPlayersInView
            = new();

        private ProtoCharacterSkeleton protoSkeleton;

        private double protoSkeletonScale;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected ProtoCharacterMob()
        {
            var name = this.GetType().Name;
            if (!name.StartsWith("Mob", StringComparison.Ordinal))
            {
                throw new Exception("Mob class name must start with \"Mob\": " + this.GetType().Name);
            }

            this.ShortId = name.Substring("Mob".Length);
        }

        public abstract bool AiIsRunAwayFromHeavyVehicles { get; }

        public virtual double BiomaterialValueMultiplier => 1.0;

        public virtual double CorpseInteractionAreaScale => 1.0;

        public virtual ITextureResource Icon
        {
            get
            {
                this.SharedGetSkeletonProto(null, out var skeletonProto, out _);
                return ((ProtoCharacterSkeletonAnimal)skeletonProto).Icon;
            }
        }

        public virtual bool IsAvailableInCompletionist => true;

        public virtual bool IsBoss => false;

        public IReadOnlyDropItemsList LootDroplist { get; private set; }

        public abstract double MobKillExperienceMultiplier { get; }

        public override double PhysicsBodyAccelerationCoef => 5;

        public override double PhysicsBodyFriction => 10;

        public override double ServerUpdateIntervalSeconds => 0.1;

        public override string ShortId { get; }

        public override double StatHealthRegenerationPerSecond => 10.0 / 60.0; // 10 health points per minute

        /// <summary>
        /// Determines whether the creature should be automatically despawned
        /// if it's too far from the spawn location for too long.
        /// </summary>
        protected virtual bool IsAutoDespawn => true;

        protected virtual byte SoundEventsNetworkRadius => 15;

        public override void ClientDeinitialize(ICharacter gameObject)
        {
            base.ClientDeinitialize(gameObject);

            if (GetPublicState(gameObject).IsDead)
            {
                GetClientState(gameObject)
                    .CurrentProtoSkeleton?
                    .PlaySound(CharacterSound.Death, gameObject);
            }
        }

        public virtual void ServerOnDeath(ICharacter character)
        {
            this.ServerSendDeathSoundEvent(character);

            ServerWorld.DestroyObject(character);

            var position = character.Position;
            var tilePosition = position.ToVector2Ushort();
            var rotationAngleRad = character.GetPublicState<ICharacterPublicState>().AppliedInput.RotationAngleRad;
            var isLeftOrientation = ClientCharacterAnimationHelper.IsLeftHalfOfCircle(
                angleDeg: rotationAngleRad * MathConstants.RadToDeg);
            var isFlippedHorizontally = !isLeftOrientation;

            var objectCorpse = ServerWorld.CreateStaticWorldObject<ObjectCorpse>(tilePosition);
            ObjectCorpse.ServerSetupCorpse(objectCorpse,
                                           (IProtoCharacterMob)character.ProtoCharacter,
                                           (Vector2F)(position - tilePosition.ToVector2D()),
                                           isFlippedHorizontally: isFlippedHorizontally);
        }

        public void ServerPlaySound(ICharacter characterNpc, CharacterSound characterSound)
        {
            using var observers = Api.Shared.GetTempList<ICharacter>();

            if (characterSound != CharacterSound.Aggression)
            {
                ServerWorld.GetScopedByPlayers(characterNpc, observers);
            }
            else
            {
                // this event is propagated on a larger distance and has sound cues
                ServerWorld.GetCharactersInRadius(characterNpc.TilePosition,
                                                  observers,
                                                  radius: this.SoundEventsNetworkRadius,
                                                  onlyPlayers: true);
            }

            this.CallClient(observers.AsList(),
                            _ => _.ClientRemote_PlaySound(characterNpc,
                                                          characterNpc.TilePosition,
                                                          characterSound));
        }

        public override IEnumerable<IItemsContainer> SharedEnumerateAllContainers(
            ICharacter character,
            bool includeEquipmentContainer)
        {
            // no containers at mobs by default
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
            if (IsServer
                && (!weaponCache.Character?.IsNpc ?? false))
            {
                var characterMob = (ICharacter)targetObject;
                var mobPrivateState = GetPrivateState(characterMob);
                mobPrivateState.CurrentAgroCharacter = weaponCache.Character;
                mobPrivateState.CurrentAgroTimeRemains = AgroStateDuration;
                //Logger.Dev(
                //    $"Mob damaged by player, let's agro: {targetObject} by {mobPrivateState.CurrentAgroCharacter} on {mobPrivateState.CurrentAgroTimeRemains:F2}s");
            }

            return base.SharedOnDamage(weaponCache,
                                       targetObject,
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

        protected override void ClientCreateOverlayControl(ICharacter character)
        {
            if (this.IsBoss)
            {
                Client.UI.LayoutRootChildren.Add(
                    new CharacterBossHealthbarControl(character));
                return;
            }

            base.ClientCreateOverlayControl(character);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            ClientCharacterEquipmentHelper.ClientRebuildAppearance(
                data.GameObject,
                data.ClientState,
                data.PublicState,
                data.PublicState.SelectedItem);
        }

        protected sealed override void PrepareProtoCharacter()
        {
            base.PrepareProtoCharacter();
            var lootDroplist = new DropItemsList();
            this.protoSkeletonScale = 1.0;
            this.PrepareProtoCharacterMob(out this.protoSkeleton, ref this.protoSkeletonScale, lootDroplist);
            this.LootDroplist = lootDroplist.AsReadOnly();
        }

        protected abstract void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist);

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
            var publicState = data.PublicState;
            var privateState = data.PrivateState;

            this.ServerInitializeCharacterMob(data);

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
            else if (publicState.IsDead)
            {
                // should never happen as the ServerCharacterDeathMechanic should properly destroy the character
                Logger.Error("(Should never happen) Destroying dead mob character: " + character);
                this.ServerOnDeath(character);
            }
        }

        protected virtual void ServerInitializeCharacterMob(ServerInitializeData data)
        {
        }

        protected void ServerSendDeathSoundEvent(ICharacter character)
        {
            using var tempList = Api.Shared.GetTempList<ICharacter>();
            ServerWorld.GetScopedByPlayers(character, tempList);
            this.CallClient(tempList.AsList(),
                            _ => _.ClientRemote_OnCharacterMobDeath(character.Position));
        }

        protected void ServerSetMobInput(
            ICharacter character,
            Vector2F movementDirection,
            double rotationAngleRad)
        {
            Vector2D movementDirectionNormalized;

            double moveSpeed;
            if (movementDirection.X != 0
                || movementDirection.Y != 0)
            {
                moveSpeed = character.SharedGetFinalStatValue(StatName.MoveSpeed);
                moveSpeed *= ProtoTile.SharedGetTileMoveSpeedMultiplier(character.Tile);
                movementDirectionNormalized = movementDirection.ToVector2D().Normalized;
            }
            else
            {
                moveSpeed = 0;
                movementDirectionNormalized = default;
            }

            Server.World.SetDynamicObjectMoveSpeed(character, moveSpeed);

            var moveAcceleration = movementDirectionNormalized * this.PhysicsBodyAccelerationCoef * moveSpeed;

            Server.World.SetDynamicObjectPhysicsMovement(character,
                                                         moveAcceleration,
                                                         targetVelocity: moveSpeed);

            var statePublic = GetPublicState(character);
            statePublic.AppliedInput.Set(
                new CharacterInput()
                {
                    MoveModes =
                        CharacterMoveModesHelper.GetPrevailingMoveModesForDirection(movementDirectionNormalized),
                    RotationAngleRad = (float)rotationAngleRad
                },
                moveSpeed);
        }

        protected sealed override void ServerUpdate(ServerUpdateData data)
        {
            var character = data.GameObject;
            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            if (publicState.IsDead)
            {
                return;
            }

            this.ServerRebuildFinalCacheIfNeeded(privateState, publicState);

            ServerUpdateAgroState(privateState, data.DeltaTime);
            this.ServerUpdateMob(data);
            this.ServerUpdateMobDespawn(character, privateState, data.DeltaTime);

            // update weapon state (fires the weapon if needed)
            WeaponSystem.SharedUpdateCurrentWeapon(
                character,
                privateState.WeaponState,
                data.DeltaTime);
        }

        protected virtual void ServerUpdateMob(ServerUpdateData data)
        {
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.GameObject.PhysicsBody.Friction = this.PhysicsBodyFriction;

            if (GetPublicState(data.GameObject).IsDead)
            {
                // do not create any physic shapes for dead character
                return;
            }

            base.SharedCreatePhysics(data);
        }

        protected sealed override void SharedGetSkeletonProto(
            [CanBeNull] ICharacter character,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale)
        {
            protoSkeleton = this.protoSkeleton;
            scale = this.protoSkeletonScale;
        }

        private static void ServerUpdateAgroState(TPrivateState privateState, double deltaTime)
        {
            if (privateState.CurrentAgroTimeRemains <= 0)
            {
                return;
            }

            var newAgroTime = privateState.CurrentAgroTimeRemains - deltaTime;
            if (newAgroTime <= 0)
            {
                // reset agro state
                newAgroTime = 0;
                privateState.CurrentAgroCharacter = null;
                //Logger.Dev("Agro state reset: " + privateState.GameObject);
            }

            privateState.CurrentAgroTimeRemains = newAgroTime;
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_OnCharacterMobDeath(Vector2D deathPosition)
        {
            var worldPosition = deathPosition + this.SharedGetObjectCenterWorldOffset(null);
            this.protoSkeleton.SoundPresetCharacter.PlaySound(CharacterSound.Death, worldPosition);
        }

        [RemoteCallSettings(DeliveryMode.Unreliable)]
        private void ClientRemote_PlaySound(
            ICharacter characterNpc,
            Vector2Ushort fallbackSoundPosition,
            CharacterSound characterSound)
        {
            Vector2D soundCuePosition;
            if (characterNpc is null
                || !characterNpc.IsInitialized)
            {
                this.protoSkeleton.SoundPresetCharacter.PlaySound(characterSound, fallbackSoundPosition.ToVector2D());
                soundCuePosition = fallbackSoundPosition.ToVector2D();
            }
            else
            {
                this.protoSkeleton.SoundPresetCharacter.PlaySound(characterSound, characterNpc);
                soundCuePosition = characterNpc.Position;
            }

            if (characterSound != CharacterSound.Aggression)
            {
                return;
            }

            // add sound cues for the aggression sounds
            for (var i = 0; i < 3; i++)
            {
                ClientTimersSystem.AddAction(delaySeconds: i * 0.1,
                                             () => ClientSoundCueManager.OnSoundEvent(soundCuePosition,
                                                 isFriendly: false));
            }
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

        private void ServerUpdateMobDespawn(ICharacter characterMob, TPrivateState privateState, double deltaTime)
        {
            if (!this.IsAutoDespawn)
            {
                return;
            }

            var distanceToSpawnSqr = privateState.SpawnPosition
                                                 .TileSqrDistanceTo(characterMob.TilePosition);
            if (distanceToSpawnSqr < DespawnTileDistanceThreshold * DespawnTileDistanceThreshold)
            {
                // close to spawn area, no need to despawn
                privateState.TimerDespawn = 0;
                return;
            }

            // too far from spawn area, should despawn after delay
            if (privateState.TimerDespawn < DespawnTimeThreshold)
            {
                // delay is not finished yet
                privateState.TimerDespawn += deltaTime;
                return;
            }

            // should despawn
            // check that nobody is observing the mob
            var playersInView = TempListPlayersInView;
            playersInView.Clear();
            ServerWorld.GetCharactersInView(characterMob,
                                            playersInView,
                                            onlyPlayerCharacters: true);

            foreach (var playerCharacter in playersInView)
            {
                if (playerCharacter.ServerIsOnline)
                {
                    // cannot despawn - scoped by a player
                    return;
                }
            }

            // nobody is observing, can despawn
            Logger.Important("Mob despawned as it went too far from the spawn position for too long: " + characterMob);
            ServerWorld.DestroyObject(characterMob);
        }
    }

    public abstract class ProtoCharacterMob
        : ProtoCharacterMob
            <CharacterMobPrivateState,
                CharacterMobPublicState,
                CharacterMobClientState>
    {
    }
}