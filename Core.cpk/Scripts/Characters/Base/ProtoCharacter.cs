namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    /// <summary>
    /// Base class for character types. Inherits from world object type.
    /// </summary>
    public abstract class ProtoCharacter
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoWorldObject
          <ICharacter,
              TPrivateState,
              TPublicState,
              TClientState>,
          IDamageableProtoWorldObject, IProtoCharacterCore
        where TPrivateState : BaseCharacterPrivateState, new()
        where TPublicState : BasePublicState, ICharacterPublicState, new()
        where TClientState : BaseCharacterClientState, new()
    {
        /// <summary>
        /// How high the character name and healthbar will be displayed in the world. Higher value means higher on screen.
        /// </summary>
        public abstract float CharacterWorldHeight { get; }

        public virtual float CharacterWorldWeaponOffsetMelee => this.CharacterWorldWeaponOffsetRanged;

        public virtual float CharacterWorldWeaponOffsetRanged => this.CharacterWorldHeight * 0.4f;

        public override double ClientUpdateIntervalSeconds => 0; // every frame

        public virtual double ObstacleBlockDamageCoef => 1;

        public abstract double PhysicsBodyAccelerationCoef { get; }

        public abstract double PhysicsBodyFriction { get; }

        public IReadOnlyStatsDictionary ProtoCharacterDefaultEffects { get; private set; }

        public override double ServerUpdateIntervalSeconds => 0; // every frame

        public override double ServerUpdateRareIntervalSeconds => 2; // every 2 seconds (for offline players usually)

        public virtual double StatDefaultHealthMax => 100;

        public abstract double StatHealthRegenerationPerSecond { get; }

        /// <inheritdoc />
        public abstract double StatMoveSpeed { get; }

        /// <summary>
        /// Used when the character is in the run mode as multiplier to its speed.
        /// </summary>
        public virtual double StatMoveSpeedRunMultiplier => 2;

        public IProtoCharacterSkeleton ClientGetCurrentProtoSkeleton(ICharacter character)
        {
            return GetClientState(character).CurrentProtoSkeleton;
        }

        public virtual MoveItemsResult ClientTryTakeAllItems(
            ICharacter character,
            IItemsContainer fromContainer,
            bool showNotificationIfInventoryFull = true)
        {
            throw new NotImplementedException();
        }

        public abstract IEnumerable<IItemsContainer> SharedEnumerateAllContainers(
            ICharacter character,
            bool includeEquipmentContainer);

        public IItemsContainerProvider SharedGetItemContainersProvider(
            ICharacter character,
            bool includeEquipmentContainer)
        {
            return new CharacterContainersProvider(character, includeEquipmentContainer);
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0, this.CharacterWorldHeight / 2.0);
        }

        public ObjectMaterial SharedGetObjectMaterialForCharacter(ICharacter character)
        {
            if (character.IsNpc)
            {
                return this.ObjectMaterial;
            }

            var equipment = character.SharedGetPlayerContainerEquipment();

            // find chest of full body armor and return its sound material
            foreach (var item in equipment.Items)
            {
                if (item.ProtoGameObject is IProtoItemEquipmentArmor protoChest)
                {
                    return protoChest.Material;
                }
            }

            return this.ObjectMaterial;
        }

        public double SharedGetRotationAngleRad(ICharacter character)
        {
            return GetPublicState(character)
                   .AppliedInput
                   .RotationAngleRad;
        }

        public void SharedGetSkeletonProto(
            ICharacter character,
            out IProtoCharacterSkeleton skeleton,
            out double scale)
        {
            scale = 1;
            this.SharedGetSkeletonProto(character, out var protoSkeleton, ref scale);
            skeleton = protoSkeleton ?? throw new NullReferenceException("No proto skeleton provided for " + character);
        }

        public virtual Vector2D SharedGetWeaponFireWorldPosition(ICharacter character, bool isMeleeWeapon)
        {
            return character.Position
                   + (0, isMeleeWeapon
                             ? character.ProtoCharacter.CharacterWorldWeaponOffsetMelee
                             : character.ProtoCharacter.CharacterWorldWeaponOffsetRanged);
        }

        public virtual bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            double damagePostMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            var targetCharacter = (ICharacter)targetObject;
            WeaponDamageSystem.SharedTryDamageCharacter(
                targetCharacter,
                weaponCache,
                damagePreMultiplier,
                damagePostMultiplier,
                out var isHit,
                out damageApplied);

            if (!isHit)
            {
                obstacleBlockDamageCoef = 0;
                return false;
            }

            obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
            if (IsClient)
            {
                this.PlaySound(CharacterSound.DamageTaken, targetCharacter);
            }

            return true;
        }

        public abstract void SharedRefreshFinalCacheIfNecessary(ICharacter character);

        public void SharedReinitializeDefaultEffects()
        {
            var effects = new Effects();
            this.FillDefaultEffects(effects);
            this.ProtoCharacterDefaultEffects = effects.ToReadOnly();
        }

        protected virtual void ClientCreateOverlayControl(ICharacter character)
        {
            Client.UI.AttachControl(
                character,
                new CharacterOverlayControl(character),
                positionOffset: (0, character.ProtoCharacter.CharacterWorldHeight),
                isFocusable: false);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var character = data.GameObject;
            var clientState = data.ClientState;
            var publicState = data.PublicState;

            if (!character.IsCurrentClientCharacter)
            {
                this.ClientCreateOverlayControl(character);
            }

            publicState.ClientSubscribe(_ => _.IsDead,
                                        isDead =>
                                        {
                                            if (isDead)
                                            {
                                                // reset the character position and movement on death
                                                character.PhysicsBody.Reset();
                                            }
                                        },
                                        clientState);

            // create loop sound emitters
            clientState.SoundEmitterLoopCharacter = Client.Audio.CreateSoundEmitter(
                character,
                SoundResource.NoSound,
                isLooped: true);

            clientState.SoundEmitterLoopMovemement = Client.Audio.CreateSoundEmitter(
                character,
                SoundResource.NoSound,
                isLooped: true);
        }

        protected sealed override void ClientOnObjectDestroyed(Vector2D position)
        {
            // we don't use this - we play CharacterSound.Death instead
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            base.ClientUpdate(data);
            var character = data.GameObject;
            var clientState = data.ClientState;
            var publicState = data.PublicState;

            ClientCharacterAnimationHelper.ClientUpdateAnimation(
                character,
                clientState,
                publicState);
        }

        protected virtual void FillDefaultEffects(Effects effects)
        {
            effects.AddValue(this, StatName.HealthMax, this.StatDefaultHealthMax)
                   .AddValue(this, StatName.HealthRegenerationPerSecond, this.StatHealthRegenerationPerSecond)
                   .AddValue(this, StatName.MoveSpeed,                   this.StatMoveSpeed)
                   .AddValue(this, StatName.MoveSpeedRunMultiplier,      this.StatMoveSpeedRunMultiplier);
        }

        protected void PlaySound(CharacterSound soundKey, ICharacter character)
        {
            ProtoCharacterSkeleton protoCharacterSkeleton;
            if (character.IsInitialized)
            {
                var clientState = GetClientState(character);
                protoCharacterSkeleton = clientState.CurrentProtoSkeleton;
            }
            else
            {
                this.SharedGetSkeletonProto(null, out var protoCharacterSkeleton1, out _);
                protoCharacterSkeleton = (ProtoCharacterSkeleton)protoCharacterSkeleton1;
            }

            protoCharacterSkeleton.PlaySound(soundKey, character);
        }

        protected virtual void PrepareProtoCharacter()
        {
        }

        protected sealed override void PrepareProtoWorldObject()
        {
            base.PrepareProtoWorldObject();

            this.SharedReinitializeDefaultEffects();

            this.PrepareProtoCharacter();
        }

        protected sealed override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            var publicState = data.PublicState;
            var privateState = data.PrivateState;
            privateState.EnsureEverythingCreated();
            publicState.ServerEnsureEverythingCreated();

            publicState.IsDead = publicState.CurrentStats.HealthCurrent <= 0;
            ServerCharacterDeathMechanic.OnCharacterInitialize(data.GameObject);

            this.ServerPrepareCharacter(data);

            if (data.IsFirstTimeInit)
            {
                this.ServerInitializeCharacterFirstTime(data);
            }

            this.ServerInitializeCharacter(data);
        }

        /// <summary>
        /// This method is invoked when the character is completely prepared and initialized.
        /// </summary>
        protected virtual void ServerInitializeCharacter(ServerInitializeData data)
        {
            SharedCharacterStatsHelper.RefreshCharacterFinalStatsCache(this.ProtoCharacterDefaultEffects,
                                                                       data.PublicState,
                                                                       data.PrivateState,
                                                                       isFirstTime: data.IsFirstTimeInit);
        }

        /// <summary>
        /// This method is invoked when the character is completely prepared but not yet initialized and was never initialized.
        /// </summary>
        protected virtual void ServerInitializeCharacterFirstTime(ServerInitializeData data)
        {
        }

        /// <summary>
        /// Ensure that all the state data is properly created.
        /// </summary>
        protected virtual void ServerPrepareCharacter(ServerInitializeData data)
        {
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var scale = 1.0;
            this.SharedGetSkeletonProto(data.GameObject, out var skeleton, ref scale);
            skeleton.CreatePhysics(data.PhysicsBody);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (scale != 1.0)
            {
                // apply non-default scale
                data.PhysicsBody.ApplyShapesScale(scale);
            }
        }

        protected abstract void SharedGetSkeletonProto(
            [CanBeNull] ICharacter character,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale);
    }
}