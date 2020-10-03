namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    // partial class containing only input methods
    public static class CharacterStatusEffectsExtensions
    {
        private static readonly ILogger Logger = Api.Logger;

        private static readonly ICharactersServerService ServerCharacters = Api.IsServer
                                                                                ? Api.Server.Characters
                                                                                : null;

        private static readonly IWorldServerService ServerWorld = Api.IsServer
                                                                      ? Api.Server.World
                                                                      : null;

        public static void ServerAddStatusEffect(
            this ICharacter character,
            IProtoStatusEffect protoStatusEffect,
            double intensity = 1.0)
        {
            if (protoStatusEffect is null)
            {
                throw new ArgumentNullException(nameof(protoStatusEffect));
            }

            if (intensity <= 0)
            {
                throw new ArgumentException(
                    $"Intensity to add must be > 0. Provided value: {intensity:F2}",
                    nameof(intensity));
            }

            if (character.ProtoCharacter is PlayerCharacterSpectator
                || ServerCharacters.IsSpectator(character))
            {
                // don't add status effects to the spectator characters
                return;
            }

            if (intensity > 1)
            {
                // clamp intensity to add
                intensity = 1;
            }

            ILogicObject statusEffect = null;

            var statusEffects = InternalServerGetStatusEffects(character);
            foreach (var existingStatusEffect in statusEffects)
            {
                if (existingStatusEffect.ProtoGameObject == protoStatusEffect)
                {
                    statusEffect = existingStatusEffect;
                    break;
                }
            }

            if (statusEffect is null)
            {
                // no such status effect instance exists - create and add it
                statusEffect = ServerWorld.CreateLogicObject(protoStatusEffect);
                protoStatusEffect.ServerSetup(statusEffect, character);
                statusEffects.Add(statusEffect);
                Logger.Info($"Status effect added: {statusEffect} to {character}");
            }

            protoStatusEffect.ServerAddIntensity(statusEffect, intensity);

            var publicState = statusEffect.GetPublicState<StatusEffectPublicState>();

            var damageContext = CharacterDamageContext.Current;
            var byCharacter = damageContext.AttackerCharacter;
            if (!ReferenceEquals(byCharacter, null))
            {
                publicState.ServerStatusEffectWasAddedByCharacter = byCharacter;
                publicState.ServerStatusEffectWasAddedByCharacterWeaponSkill = damageContext.ProtoWeaponSkill;
            }
        }

        public static void ServerAddStatusEffect<TProtoStatusEffect>(
            this ICharacter character,
            double intensity = 1.0)
            where TProtoStatusEffect : class, IProtoStatusEffect, new()
        {
            var protoStatusEffect = Api.GetProtoEntity<TProtoStatusEffect>();
            character.ServerAddStatusEffect(protoStatusEffect, intensity);
        }

        public static IEnumerable<ILogicObject> ServerEnumerateCurrentStatusEffects(this ICharacter character)
        {
            return InternalServerGetStatusEffects(character);
        }

        public static void ServerRemoveAllStatusEffects(this ICharacter character, bool removeOnlyDebuffs = false)
        {
            var statusEffects = InternalServerGetStatusEffects(character);
            for (var index = 0; index < statusEffects.Count; index++)
            {
                var statusEffect = statusEffects[index];
                if (removeOnlyDebuffs
                    && ((IProtoStatusEffect)statusEffect.ProtoLogicObject).Kind == StatusEffectKind.Buff)
                {
                    continue;
                }

                ServerWorld.DestroyObject(statusEffect);
                statusEffects.RemoveAt(index);
                index--;
            }

            Logger.Important("All status effects removed for " + character);
        }

        public static void ServerRemoveStatusEffect<TProtoStatusEffect>(this ICharacter character)
            where TProtoStatusEffect : class, IProtoStatusEffect, new()
        {
            var protoStatusEffect = Api.GetProtoEntity<TProtoStatusEffect>();
            character.ServerRemoveStatusEffect(protoStatusEffect);
        }

        public static void ServerRemoveStatusEffect(
            this ICharacter character,
            IProtoStatusEffect protoStatusEffect)
        {
            if (protoStatusEffect is null)
            {
                throw new ArgumentNullException(nameof(protoStatusEffect));
            }

            var statusEffects = InternalServerGetStatusEffects(character);
            for (var index = 0; index < statusEffects.Count; index++)
            {
                var statusEffect = statusEffects[index];
                if (statusEffect.ProtoGameObject != protoStatusEffect)
                {
                    continue;
                }

                // found effect to remove
                statusEffects.RemoveAt(index);
                ServerWorld.DestroyObject(statusEffect);
                Logger.Info($"Status effect removed: {statusEffect} from {character}");
                return;
            }

            // no status effect found
            Logger.Warning($"Cannot remove status effect: {protoStatusEffect} from {character}");
        }

        public static void ServerRemoveStatusEffectIntensity<TProtoStatusEffect>(
            this ICharacter character,
            double intensityToRemove)
            where TProtoStatusEffect : class, IProtoStatusEffect, new()
        {
            var protoStatusEffect = Api.GetProtoEntity<TProtoStatusEffect>();
            character.ServerRemoveStatusEffectIntensity(protoStatusEffect, intensityToRemove);
        }

        public static void ServerRemoveStatusEffectIntensity(
            this ICharacter character,
            IProtoStatusEffect protoStatusEffect,
            double intensityToRemove)
        {
            if (protoStatusEffect is null)
            {
                throw new ArgumentNullException(nameof(protoStatusEffect));
            }

            if (intensityToRemove <= 0)
            {
                throw new ArgumentException("Intensity to remove must be > 0", nameof(intensityToRemove));
            }

            var statusEffects = InternalServerGetStatusEffects(character);
            foreach (var statusEffect in statusEffects)
            {
                if (statusEffect.ProtoGameObject != protoStatusEffect)
                {
                    continue;
                }

                // found effect to reduce intensity
                var state = statusEffect.GetPublicState<StatusEffectPublicState>();
                state.SetIntensity(state.Intensity - intensityToRemove);
                return;
            }
        }

        public static void ServerSetStatusEffectIntensity<TProtoStatusEffect>(
            this ICharacter character,
            double intensity)
            where TProtoStatusEffect : class, IProtoStatusEffect, new()
        {
            var protoStatusEffect = Api.GetProtoEntity<TProtoStatusEffect>();
            character.ServerSetStatusEffectIntensity(protoStatusEffect, intensity);
        }

        public static void ServerSetStatusEffectIntensity(
            this ICharacter character,
            IProtoStatusEffect protoStatusEffect,
            double intensity)
        {
            if (protoStatusEffect is null)
            {
                throw new ArgumentNullException(nameof(protoStatusEffect));
            }

            var statusEffects = InternalServerGetStatusEffects(character);
            foreach (var statusEffect in statusEffects)
            {
                if (statusEffect.ProtoGameObject != protoStatusEffect)
                {
                    continue;
                }

                // found effect to set intensity
                var state = statusEffect.GetPublicState<StatusEffectPublicState>();
                state.SetIntensity(intensity);
                return;
            }

            // status effect instance is not found
            if (intensity > 0)
            {
                ServerAddStatusEffect(character, protoStatusEffect, intensity);
            }
        }

        public static double SharedGetStatusEffectIntensity<TProtoStatusEffect>(
            this ICharacter character)
            where TProtoStatusEffect : class, IProtoStatusEffect, new()
        {
            var protoStatusEffect = Api.GetProtoEntity<TProtoStatusEffect>();
            return character.SharedGetStatusEffectIntensity(protoStatusEffect);
        }

        public static double SharedGetStatusEffectIntensity(
            this ICharacter character,
            IProtoStatusEffect protoStatusEffect)
        {
            if (protoStatusEffect is null)
            {
                throw new ArgumentNullException(nameof(protoStatusEffect));
            }

            var statusEffects = InternalSharedGetStatusEffects(character);
            foreach (var statusEffect in statusEffects)
            {
                if (statusEffect.ProtoGameObject != protoStatusEffect)
                {
                    continue;
                }

                if (Api.IsClient
                    && statusEffect.IsDestroyed)
                {
                    // the status effect might be already destroyed but its intensity could be > 0
                    return 0;
                }

                // found effect to set intensity
                var state = statusEffect.GetPublicState<StatusEffectPublicState>();
                return state.Intensity;
            }

            // status effect not found - intensity 0
            return 0;
        }

        public static bool SharedHasStatusEffect<TProtoStatusEffect>(
            this ICharacter character,
            double minIntensity = 0,
            double maxIntensity = 1)
            where TProtoStatusEffect : class, IProtoStatusEffect
        {
            var statusEffects = InternalSharedGetStatusEffects(character);
            foreach (var statusEffect in statusEffects)
            {
                if (!(statusEffect.ProtoGameObject is TProtoStatusEffect))
                {
                    continue;
                }

                var intensity = statusEffect.GetPublicState<StatusEffectPublicState>()
                                            .Intensity;

                if (intensity < minIntensity
                    && intensity > maxIntensity)
                {
                    // the status effect intensity is out of the required range
                    continue;
                }

                // has this status effect
                return true;
            }

            // don't have such status effect
            return false;
        }

        private static NetworkSyncList<ILogicObject> InternalServerGetStatusEffects(ICharacter character)
        {
            Api.ValidateIsServer();
            return InternalSharedGetStatusEffects(character);
        }

        private static NetworkSyncList<ILogicObject> InternalSharedGetStatusEffects(ICharacter character)
        {
            return character.GetPrivateState<BaseCharacterPrivateState>()
                            .StatusEffects;
        }
    }
}