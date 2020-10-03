namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using JetBrains.Annotations;

    public class EffectAction
    {
        public EffectAction(
            IProtoStatusEffect protoStatusEffect,
            double intensity,
            [CanBeNull] DelegeteEffectActionCondition condition = null,
            bool isHidden = false)
        {
            this.Condition = condition;
            this.IsHidden = isHidden;
            this.ProtoStatusEffect = protoStatusEffect;
            Api.Assert(intensity != 0, "Intensity cannot be 0");
            this.Intensity = MathHelper.Clamp(intensity, -1, 1);
        }

        public DelegeteEffectActionCondition Condition { get; }

        public double Intensity { get; }

        public bool IsHidden { get; }

        public IProtoStatusEffect ProtoStatusEffect { get; }

        public void Execute(EffectActionContext context)
        {
            try
            {
                if (context.Character is null)
                {
                    throw new Exception("Character cannot be null in the " + nameof(EffectActionContext));
                }

                if (this.Condition is not null
                    && !this.Condition(context))
                {
                    return;
                }

                if (this.Intensity > 0)
                {
                    context.Character.ServerAddStatusEffect(this.ProtoStatusEffect, this.Intensity);
                }
                else
                {
                    context.Character.ServerRemoveStatusEffectIntensity(this.ProtoStatusEffect, -this.Intensity);
                }
            }
            catch (Exception ex)
            {
                Api.Logger.Exception(ex);
            }
        }
    }
}