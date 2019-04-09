namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public static class ProtoSkillExtensions
    {
        public static DropItemConditionDelegate ToCondition<TFlagEffectsEnum>(this TFlagEffectsEnum flag)
            where TFlagEffectsEnum : struct, Enum
        {
            return context => context.HasCharacter
                              && context.Character.SharedHasSkillFlag(flag);
        }
    }
}