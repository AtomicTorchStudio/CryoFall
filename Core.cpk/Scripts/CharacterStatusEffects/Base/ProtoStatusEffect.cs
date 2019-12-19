namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Extensions;

    [PrepareOrder(afterType: typeof(IProtoTrigger))]
    public abstract class ProtoStatusEffect
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoGameObject
          <ILogicObject,
              TPrivateState,
              TPublicState,
              TClientState>,
          IProtoStatusEffect
        where TPrivateState : BasePrivateState, new()
        where TPublicState : StatusEffectPublicState, new()
        where TClientState : BaseClientState, new()
    {
        private bool isAutoDecrease;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected ProtoStatusEffect()
        {
            var name = this.GetType().Name;
            if (name.StartsWith("StatusEffect"))
            {
                name = name.Substring("StatusEffect".Length);
            }

            this.ShortId = name;
            this.Icon = new TextureResource($"StatusEffects/{name}.png");
        }

        public override double ClientUpdateIntervalSeconds => 0; // every frame

        public abstract string Description { get; }

        public bool HasStatEffects { get; private set; }

        public virtual ITextureResource Icon { get; }

        public virtual double IntensityAutoDecreasePerSecondFraction => 0;

        public virtual double IntensityAutoDecreasePerSecondValue => 0;

        public virtual bool IsIntensityPercentVisible => true;

        public virtual bool IsRemovedOnRespawn => true;

        public abstract StatusEffectKind Kind { get; }

        public IReadOnlyStatsDictionary ProtoEffects { get; private set; }

        /// <summary>
        /// Please note: this used only if the method ServerOnAutoAdd has an override.
        /// </summary>
        public virtual double ServerAutoAddRepeatIntervalSeconds => 1;

        public override double ServerUpdateIntervalSeconds => 1;

        public override string ShortId { get; }

        public virtual double VisibilityIntensityThreshold => 0;

        public sealed override void ClientDeinitialize(ILogicObject gameObject)
        {
            if (Client.Characters.IsCurrentPlayerCharacterSpectator)
            {
                return;
            }

            var data = new StatusEffectData(gameObject, deltaTime: 0);
            this.ClientDeinitialize(data);

            if (this.HasStatEffects)
            {
                data.CharacterPrivateState.SetFinalStatsCacheIsDirty();
            }
        }

        public void ServerAddIntensity(ILogicObject statusEffect, double intensityToAdd)
        {
            var effectData = new StatusEffectData(statusEffect, deltaTime: 0);
            this.ServerAddIntensity(effectData, intensityToAdd);

            if (effectData.Intensity <= 0)
            {
                // didn't add any intensity or intensity is zero - delete this status effect
                effectData.Intensity = 0;
                return;
            }

            Logger.Info(
                $"Status effect intensity added: {statusEffect}: intensity {effectData.StatusEffectPublicState.Intensity:F3} (added {intensityToAdd:F3}) to {effectData.Character}");

            this.ServerRefreshPublicStatusEffectEntry(effectData);
        }

        public sealed override void ServerOnDestroy(ILogicObject gameObject)
        {
            var statusEffectData = new StatusEffectData(gameObject, deltaTime: 0);
            this.ServerDestroy(statusEffectData);

            var character = statusEffectData.Character;
            if (character == null
                || character.IsDestroyed)
            {
                return;
            }

            if (this.HasStatEffects
                && !character.IsDestroyed)
            {
                statusEffectData.CharacterPrivateState.SetFinalStatsCacheIsDirty();
            }

            // try remove the public status effect entry
            this.ServerRefreshPublicStatusEffectEntry(statusEffectData);
        }

        public void ServerSetup(ILogicObject statusEffect, ICharacter character)
        {
            GetPublicState(statusEffect).Setup(character);

            if (this.HasStatEffects)
            {
                character.SharedSetFinalStatsCacheDirty();
            }

            this.ServerSetup(new StatusEffectData(statusEffect, deltaTime: 0));
        }

        protected virtual void ClientDeinitialize(StatusEffectData data)
        {
            Logger.Info($"Status effect destroyed: {data.StatusEffect} for {data.Character}");
        }

        protected sealed override void ClientInitialize(ClientInitializeData data)
        {
            if (Client.Characters.IsCurrentPlayerCharacterSpectator)
            {
                return;
            }

            var statusEffectData = new StatusEffectData(data.GameObject, deltaTime: 0);
            this.ClientInitialize(statusEffectData);

            if (this.HasStatEffects)
            {
                statusEffectData.CharacterPrivateState.SetFinalStatsCacheIsDirty();
            }
        }

        protected virtual void ClientInitialize(StatusEffectData data)
        {
            Logger.Info($"Status effect initialized: {data.StatusEffect} for {data.Character}");
        }

        protected sealed override void ClientUpdate(ClientUpdateData data)
        {
            if (Client.Characters.IsCurrentPlayerCharacterSpectator)
            {
                return;
            }

            this.ClientUpdate(new StatusEffectData(data.GameObject, data.DeltaTime));
        }

        protected virtual void ClientUpdate(StatusEffectData data)
        {
        }

        protected virtual void PrepareEffects(Effects effects)
        {
        }

        protected sealed override void PrepareProto()
        {
            var effects = new Effects();
            this.PrepareEffects(effects);
            this.ProtoEffects = effects.ToReadOnly();

            this.PrepareProtoStatusEffect();

            if (this.IntensityAutoDecreasePerSecondFraction < 0
                || this.IntensityAutoDecreasePerSecondFraction > 1)
            {
                throw new Exception(
                    $"Invalid value of the property {nameof(this.IntensityAutoDecreasePerSecondFraction)}: it must be in [0;1] range");
            }

            this.isAutoDecrease = this.IntensityAutoDecreasePerSecondValue > 0
                                  || this.IntensityAutoDecreasePerSecondFraction > 0;

            if (IsServer 
                && this.GetType().HasOverride(nameof(this.ServerOnAutoAdd), isPublic: false))
            {
                TriggerTimeInterval.ServerConfigureAndRegister(
                    interval: TimeSpan.FromSeconds(this.ServerAutoAddRepeatIntervalSeconds),
                    callback: this.ServerAutoAddTimerCallback,
                    name: "StatusEffect.AutoAddCallback." + this.ShortId);
            }

            this.HasStatEffects = !this.ProtoEffects.IsEmpty;
        }

        protected virtual void PrepareProtoStatusEffect()
        {
        }

        protected virtual void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            var publicState = data.StatusEffectPublicState;
            publicState.SetIntensity(publicState.Intensity + intensityToAdd);
        }

        protected virtual IEnumerable<ICharacter> ServerAutoAddGetCharacterCandidates()
        {
            // by default process all player characters in the world
            return Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: false);
        }

        protected void ServerAutoAddTimerCallback()
        {
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (var character in this.ServerAutoAddGetCharacterCandidates())
            {
                this.ServerOnAutoAdd(character);
            }
        }

        protected virtual void ServerDestroy(StatusEffectData data)
        {
        }

        /// <summary>
        /// This override is sealed - use <see cref="ServerSetup" /> method instead.
        /// </summary>
        protected sealed override void ServerInitialize(ServerInitializeData data)
        {
        }

        protected virtual void ServerOnAutoAdd(ICharacter character)
        {
        }

        protected virtual void ServerOnAutoDecreaseIntensity(StatusEffectData data)
        {
            var intensity = data.Intensity;
            if (intensity <= 0)
            {
                if (!data.StatusEffect.IsDestroyed)
                {
                    data.Intensity = 0;
                }

                return;
            }

            intensity -= this.IntensityAutoDecreasePerSecondValue;

            if (this.IntensityAutoDecreasePerSecondFraction > 0)
            {
                intensity *= 1 - this.IntensityAutoDecreasePerSecondFraction;
            }

            data.Intensity = intensity;
        }

        protected virtual void ServerSetup(StatusEffectData data)
        {
        }

        protected sealed override void ServerUpdate(ServerUpdateData data)
        {
            var effectData = new StatusEffectData(data.GameObject, data.DeltaTime);
            var character = effectData.Character;

            if (character == null
                || character.IsDestroyed)
            {
                //Logger.WriteDev("The status effect owner character is no longer exist - destroy the status effect: " + data.GameObject);
                Server.World.DestroyObject(effectData.StatusEffect);
                return;
            }

            if (effectData.CharacterPublicState.IsDead)
            {
                return;
            }

            this.ServerUpdate(effectData);

            if (this.isAutoDecrease)
            {
                this.ServerOnAutoDecreaseIntensity(effectData);
            }

            this.ServerRefreshPublicStatusEffectEntry(effectData);
        }

        protected virtual void ServerUpdate(StatusEffectData data)
        {
        }

        private void ServerRefreshPublicStatusEffectEntry(in StatusEffectData effectData)
        {
            if (this.VisibilityIntensityThreshold >= 1)
            {
                // never show this effect
                return;
            }

            var effectState = effectData.StatusEffectPublicState;
            if (effectState.Intensity >= this.VisibilityIntensityThreshold
                && !effectState.GameObject.IsDestroyed)
            {
                if (effectState.ServerIsAddedToCharacterPublicState)
                {
                    return;
                }

                // add public status effect
                var publicStatusEffects = effectData.Character.GetPublicState<ICharacterPublicState>()
                                                    .CurrentPublicStatusEffects;
                publicStatusEffects.Add(this);
                effectState.ServerIsAddedToCharacterPublicState = true;
                return;
            }

            // intensity is 0
            if (effectState.ServerIsAddedToCharacterPublicState)
            {
                // remove public status effect
                var publicStatusEffects = effectData.Character.GetPublicState<ICharacterPublicState>()
                                                    .CurrentPublicStatusEffects;
                publicStatusEffects.Remove(this);
                effectState.ServerIsAddedToCharacterPublicState = false;
            }
        }

        public struct StatusEffectData
        {
            /// <summary>
            /// DeltaTime (in seconds) since previous update.
            /// </summary>
            public readonly double DeltaTime;

            /// <summary>
            /// Game object instance.
            /// </summary>
            public readonly ILogicObject StatusEffect;

            private BaseCharacterPrivateState characterPrivateState;

            private ICharacterPublicState characterPublicState;

            private TPublicState publicState;

            private TPrivateState statusEffectPrivateState;

            internal StatusEffectData(ILogicObject statusEffect, double deltaTime) : this()
            {
                this.StatusEffect = statusEffect;
                this.DeltaTime = deltaTime;
            }

            public ICharacter Character => this.StatusEffectPublicState.Character;

            public CharacterCurrentStats CharacterCurrentStats
                => this.CharacterPublicState.CurrentStats;

            public BaseCharacterPrivateState CharacterPrivateState
                => this.characterPrivateState ??= this.Character.GetPrivateState<BaseCharacterPrivateState>();

            public ICharacterPublicState CharacterPublicState
                => this.characterPublicState ??= this.Character.GetPublicState<ICharacterPublicState>();

            public double Intensity
            {
                get => this.StatusEffectPublicState.Intensity;
                set => this.StatusEffectPublicState.SetIntensity(value);
            }

            /// <summary>
            /// Server private state of this game object.
            /// </summary>
            public TPrivateState StatusEffectPrivateState
                => this.statusEffectPrivateState ??= GetPrivateState(this.StatusEffect);

            /// <summary>
            /// Server public state of this game object.
            /// </summary>
            public TPublicState StatusEffectPublicState
                => this.publicState ??= GetPublicState(this.StatusEffect);
        }
    }

    public abstract class ProtoStatusEffect
        : ProtoStatusEffect<
            EmptyPrivateState,
            StatusEffectPublicState,
            EmptyClientState>
    {
    }
}