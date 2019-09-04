namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesServer;
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

        /// <summary>
        /// Update every frame.
        /// </summary>
        public override double ClientUpdateIntervalSeconds => 0;

        public virtual double ObstacleBlockDamageCoef => 1;

        public IReadOnlyStatsDictionary ProtoCharacterDefaultEffects { get; private set; }

        /// <summary>
        /// Update every frame.
        /// </summary>
        public override double ServerUpdateIntervalSeconds => 0;

        public virtual double StatDefaultHealthMax => 100;

        public abstract double StatDefaultStaminaMax { get; }

        public abstract double StatHealthRegenerationPerSecond { get; }

        /// <inheritdoc />
        public abstract double StatMoveSpeed { get; }

        /// <summary>
        /// Used when the character is in the run mode as multiplier to its speed.
        /// </summary>
        public virtual double StatMoveSpeedRunMultiplier => 2;

        public abstract double StatStaminaRegenerationPerSecond { get; }

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

        public abstract CreateItemResult ServerCreateItem(
            ICharacter character,
            IProtoItem protoItem,
            uint countToSpawn = 1);

        public CreateItemResult ServerCreateItem<TProtoItem>(
            ICharacter character,
            uint countToSpawn = 1)
            where TProtoItem : class, IProtoItem, new()
        {
            return this.ServerCreateItem(character, GetProtoEntity<TProtoItem>(), countToSpawn);
        }

        public abstract IEnumerable<IItemsContainer> SharedEnumerateAllContainers(
            ICharacter character,
            bool includeEquipmentContainer);

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0, this.CharacterWorldHeight / 2.0);
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

        public virtual bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            var targetCharacter = (ICharacter)targetObject;
            var isHit = WeaponDamageSystem.SharedOnDamageToCharacter(
                targetCharacter,
                weaponCache,
                damagePreMultiplier,
                out damageApplied);

            if (isHit)
            {
                obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
                if (IsClient)
                {
                    this.PlaySound(CharacterSound.DamageTaken, targetCharacter);
                }
            }
            else
            {
                obstacleBlockDamageCoef = 0;
            }

            return isHit;
        }

        public abstract void SharedRefreshFinalCacheIfNecessary(ICharacter character);

        public void SharedReinitializeDefaultEffects()
        {
            var effects = new Effects();
            this.FillDefaultEffects(effects);
            this.ProtoCharacterDefaultEffects = effects.ToReadOnly();
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var character = data.GameObject;
            var clientState = data.ClientState;
            var publicState = data.PublicState;

            if (!character.IsCurrentClientCharacter)
            {
                var yOffset = character.ProtoCharacter.CharacterWorldHeight;
                clientState.HealthbarControl = Client.UI.AttachControl(
                    character,
                    new CharacterOverlayControl(character),
                    positionOffset: (0, yOffset),
                    isFocusable: false);
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

            // create shadow renderer
            this.SharedGetSkeletonProto(character, out var skeleton, out var scaleMultiplier);
            clientState.RendererShadow = ((ProtoCharacterSkeleton)skeleton)
                .ClientCreateShadowRenderer(character,
                                            scaleMultiplier);

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

        protected sealed override void ClientOnObjectDestroyed(Vector2Ushort tilePosition)
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
            effects.AddValue(this, StatName.MoveSpeed,                    this.StatMoveSpeed);
            effects.AddValue(this, StatName.MoveSpeedRunMultiplier,       this.StatMoveSpeedRunMultiplier);
            effects.AddValue(this, StatName.StaminaRegenerationPerSecond, this.StatStaminaRegenerationPerSecond);

            effects.AddValue(this, StatName.HealthRegenerationPerSecond, this.StatHealthRegenerationPerSecond);

            effects.AddValue(this, StatName.HealthMax,  this.StatDefaultHealthMax);
            effects.AddValue(this, StatName.StaminaMax, this.StatDefaultStaminaMax);
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
            publicState.EnsureEverythingCreated();

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