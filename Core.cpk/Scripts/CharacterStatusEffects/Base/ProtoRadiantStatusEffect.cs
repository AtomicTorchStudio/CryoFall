﻿namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    /// <summary>
    /// Special status effect which depends on nearby objects to increase/decrease.
    /// </summary>
    public abstract class ProtoRadiantStatusEffect : ProtoStatusEffect
    {
        /// <summary>
        /// How much a 100% armor would protect you from environmental effect.
        /// </summary>
        protected const double DefensePotentialMultiplier = 0.75;

        private static readonly List<IWorldObject> TempResult
            = new(capacity: 512);

        public sealed override double IntensityAutoDecreasePerSecondValue
            => 0; // doesn't decrease via base implementation

        public override double ServerUpdateIntervalSeconds => 0.5;

        protected abstract StatName DefenseStatName { get; }

        /// <summary>
        /// Time to remove full effect intensity back to zero in case the environmental intensity is 0.
        /// </summary>
        protected abstract double TimeToCoolDownToZeroSeconds { get; }

        /// <summary>
        /// Time to reach the full intensity in case the environmental intensity is 1.
        /// </summary>
        protected abstract double TimeToReachFullIntensitySeconds { get; }

        protected override void PrepareProtoStatusEffect()
        {
            base.PrepareProtoStatusEffect();

            if (Api.IsClient)
            {
                return;
            }

            // setup timer (tick every frame)
            TriggerEveryFrame.ServerRegister(
                callback: this.ServerGlobalUpdate,
                name: "System." + this.ShortId);
        }

        protected virtual double ServerCalculateEnvironmentalIntensityAroundCharacter(ICharacter character)
        {
            Server.World.GetWorldObjectsInView(character, TempResult, sortByDistance: false);

            var result = 0.0;
            foreach (var worldObject in TempResult)
            {
                var environmentalIntensity = this.ServerCalculateObjectEnvironmentalIntensity(character,
                                                                                              worldObject);

                if (result < environmentalIntensity)
                {
                    result = environmentalIntensity;
                }
            }

            return result;
        }

        protected abstract double ServerCalculateObjectEnvironmentalIntensity(
            ICharacter character,
            IWorldObject worldObject);

        protected override void ServerUpdate(StatusEffectData data)
        {
            this.ServerUpdateRadiantStatusEffectIntensity(data.Character, data.DeltaTime);
        }

        private void ServerGlobalUpdate()
        {
            // perform update once per second per player
            const double spreadDeltaTime = 1;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            PlayerCharacter.Instance
                           .EnumerateGameObjectsWithSpread(tempListPlayers.AsList(),
                                                           spreadDeltaTime: spreadDeltaTime,
                                                           Server.Game.FrameNumber,
                                                           Server.Game.FrameRate);

            foreach (var character in tempListPlayers.AsList())
            {
                if (!character.ServerIsOnline
                    || character.GetPublicState<ICharacterPublicState>()
                                .IsDead)
                {
                    continue;
                }

                var statusEffects = character.GetPrivateState<BaseCharacterPrivateState>()
                                             .StatusEffects;

                var isEffectAlreadyAdded = false;
                foreach (var statusEffect in statusEffects)
                {
                    if (!ReferenceEquals(statusEffect.ProtoLogicObject, this))
                    {
                        continue;
                    }

                    // status effect is already added so it will be automatically updated
                    isEffectAlreadyAdded = true;
                    break;
                }

                if (isEffectAlreadyAdded)
                {
                    continue;
                }

                this.ServerUpdateRadiantStatusEffectIntensity(character, spreadDeltaTime);
            }
        }

        private void ServerUpdateRadiantStatusEffectIntensity(ICharacter character, double deltaTime)
        {
            var environmentalIntensity = character.IsNpc
                                             ? 0 // environmental intensity doesn't apply to NPCs
                                             : this.ServerCalculateEnvironmentalIntensityAroundCharacter(character);

            if (environmentalIntensity > 0)
            {
                // Armor/defense proportionally reduces the maximum environmental intensity.
                // i.e. 100% defense results in 75% reduced environmental intensity
                var defense = character.SharedGetFinalStatValue(this.DefenseStatName);
                defense = MathHelper.Clamp(defense, 0, 1);
                environmentalIntensity *= 1 - DefensePotentialMultiplier * defense;
            }

            var currentIntensity = character.SharedGetStatusEffectIntensity(this);

            var delta = environmentalIntensity - currentIntensity;
            if (delta > 0)
            {
                // need to add the intensity
                var speed = 1.0 / (environmentalIntensity * this.TimeToReachFullIntensitySeconds);
                delta = Math.Min(delta, speed * deltaTime);
            }
            else if (delta < 0)
            {
                // need to reduce the intensity
                var speed = 1.0 / ((environmentalIntensity - 1) * this.TimeToCoolDownToZeroSeconds);
                delta = Math.Max(delta, speed * deltaTime);
            }
            else
            {
                // no need to change the intensity
                return;
            }

            var newIntensity = currentIntensity + delta;
            character.ServerSetStatusEffectIntensity(this, intensity: newIntensity);
        }
    }
}