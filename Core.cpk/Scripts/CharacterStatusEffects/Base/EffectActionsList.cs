namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    public class EffectActionsList
    {
        private readonly List<EffectAction> list = new List<EffectAction>();

        public EffectActionsList Clear()
        {
            this.list.Clear();
            return this;
        }

        public IReadOnlyList<EffectAction> ToReadOnly()
        {
            return this.list;
        }

        public EffectActionsList WillAddEffect<TProtoStatusEffect>(
            double intensity = 1.0,
            [CanBeNull] DelegeteEffectActionCondition condition = null,
            bool isHidden = false)
            where TProtoStatusEffect : IProtoStatusEffect, new()
        {
            if (intensity <= 0)
            {
                throw new ArgumentException("Intensity must be > 0", nameof(intensity));
            }

            if (intensity > 1)
            {
                throw new ArgumentException("Intensity must be <= 1", nameof(intensity));
            }

            this.list.Add(
                new EffectAction(
                    Api.GetProtoEntity<TProtoStatusEffect>(),
                    intensity,
                    condition,
                    isHidden));
            return this;
        }

        public EffectActionsList WillRemoveEffect<TProtoStatusEffect>(
            double intensityToRemove = 1.0,
            [CanBeNull] DelegeteEffectActionCondition condition = null,
            bool isHidden = false)
            where TProtoStatusEffect : IProtoStatusEffect, new()
        {
            if (intensityToRemove <= 0)
            {
                throw new ArgumentException("Intensity to remove must be > 0", nameof(intensityToRemove));
            }

            if (intensityToRemove > 1)
            {
                throw new ArgumentException("Intensity to remove must be <= 1", nameof(intensityToRemove));
            }

            this.list.Add(
                new EffectAction(
                    Api.GetProtoEntity<TProtoStatusEffect>(),
                    -intensityToRemove,
                    condition,
                    isHidden));
            return this;
        }
    }
}