namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
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

        public IReadOnlyDropItemsList LootDroplist { get; private set; }

        public abstract double MobKillExperienceMultiplier { get; }

        public override double ServerUpdateIntervalSeconds => 0.1;

        public override string ShortId { get; }

        public override double StatDefaultStaminaMax => 0; // currently we don't use energy for mobs

        public override double StatHealthRegenerationPerSecond => 10.0 / 60.0; // 10 health points per minute

        public override double StatStaminaRegenerationPerSecond => 0.0; // currently we don't use energy for mobs

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

        public override CreateItemResult ServerCreateItem(
            ICharacter character,
            IProtoItem protoItem,
            uint countToSpawn = 1)
        {
            // no containers - cannot create items
            throw new NotImplementedException();
        }

        public override void ServerOnDestroy(ICharacter gameObject)
        {
            base.ServerOnDestroy(gameObject);
            if (!GetPublicState(gameObject).IsDead)
            {
                return;
            }

            using (var tempList = Api.Shared.GetTempList<ICharacter>())
            {
                Server.World.GetScopedByPlayers(gameObject, tempList);
                this.CallClient(tempList, _ => _.ClientRemote_OnCharacterMobDeath(gameObject.Position));
            }
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
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            if (IsServer
                && (!weaponCache.Character?.IsNpc ?? false))
            {
                var mobPrivateState = GetPrivateState((ICharacter)targetObject);
                mobPrivateState.CurrentAgroCharacter = weaponCache.Character;
                //Logger.Dev(
                //    $"Mob damaged by player, let's agro: {targetObject} by {mobPrivateState.CurrentAgroCharacter}");
            }

            return base.SharedOnDamage(weaponCache,
                                       targetObject,
                                       damagePreMultiplier,
                                       out obstacleBlockDamageCoef,
                                       out damageApplied);
        }

        public override void SharedRefreshFinalCacheIfNecessary(ICharacter character)
        {
            if (IsServer)
            {
                this.ServerRebuildFinalCacheIfNeeded(character,
                                                     GetPrivateState(character),
                                                     GetPublicState(character));
            }
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            ClientCharacterEquipmentHelper.ClientRebuildAppearance(
                data.GameObject,
                data.ClientState,
                data.SyncPublicState,
                data.SyncPublicState.SelectedHotbarItem);
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

        protected override void ServerInitializeCharacter(ServerInitializeData data)
        {
            // don't call the base initialize
            // base.ServerInitializeCharacter(data);

            this.ServerInitializeCharacterMob(data);

            SharedCharacterStatsHelper.RefreshCharacterFinalStatsCache(this.ProtoCharacterDefaultEffects,
                                                                       data.PublicState,
                                                                       data.PrivateState,
                                                                       isFirstTime: data.IsFirstTimeInit);

            var character = data.GameObject;
            var weaponState = data.PrivateState.WeaponState;
            WeaponSystem.RebuildWeaponCache(character, weaponState);
            data.PrivateState.AttackRange = weaponState.WeaponCache?.RangeMax ?? 0;
        }

        protected virtual void ServerInitializeCharacterMob(ServerInitializeData data)
        {
        }

        protected void ServerSetMobInput(
            ICharacter character,
            Vector2F movementDirection,
            double rotationAngleRad)
        {
            var movementDirectionNormalized = new Vector2D(
                ClampDirection(movementDirection.X),
                ClampDirection(movementDirection.Y)).Normalized;

            var moveSpeed = character.SharedGetFinalStatValue(StatName.MoveSpeed);
            moveSpeed *= ProtoTile.SharedGetTileMoveSpeedMultiplier(character.Tile);
            Server.Characters.SetMoveSpeed(character, moveSpeed);

            var moveVelocity = movementDirectionNormalized * moveSpeed;
            Server.Characters.SetVelocity(character, moveVelocity);

            var statePublic = GetPublicState(character);
            statePublic.AppliedInput.Set(
                new CharacterInput()
                {
                    MoveModes = CharacterMoveModesHelper.CalculateMoveModes(movementDirectionNormalized),
                    RotationAngleRad = (float)rotationAngleRad,
                },
                moveSpeed);

            double ClampDirection(double dir)
            {
                const double directionThreshold = 0.1;
                if (dir > directionThreshold)
                {
                    return 1;
                }

                if (dir < -directionThreshold)
                {
                    return -1;
                }

                return 0;
            }
        }

        protected sealed override void ServerUpdate(ServerUpdateData data)
        {
            var character = data.GameObject;
            var publicState = data.PublicState;

            if (publicState.IsDead)
            {
                // should never happen as the ServerCharacterDeathMechanic should properly destroy the character
                Logger.Error("(Should never happen) Destroying dead mob character: " + character);
                Server.World.DestroyObject(character);
                return;
            }

            this.ServerRebuildFinalCacheIfNeeded(character, data.PrivateState, publicState);
            this.ServerUpdateMob(data);

            // update weapon state (fires the weapon if needed)
            WeaponSystem.SharedUpdateCurrentWeapon(
                character,
                data.PrivateState.WeaponState,
                data.DeltaTime);
        }

        protected virtual void ServerUpdateMob(ServerUpdateData data)
        {
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            if (GetPublicState(data.GameObject).IsDead)
            {
                // do not create any physics for dead character
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

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_OnCharacterMobDeath(Vector2D deathPosition)
        {
            var worldPosition = deathPosition + this.SharedGetObjectCenterWorldOffset(null);
            this.protoSkeleton.SoundPresetCharacter.PlaySound(CharacterSound.Death, worldPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ServerRebuildFinalCacheIfNeeded(
            ICharacter character,
            BaseCharacterPrivateState privateState,
            ICharacterPublicState publicState)
        {
            if (!privateState.FinalStatsCache.IsDirty)
            {
                return;
            }

            // rebuild stats cache
            SharedCharacterStatsHelper.RefreshCharacterFinalStatsCache(this.ProtoCharacterDefaultEffects,
                                                                       publicState,
                                                                       privateState);
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