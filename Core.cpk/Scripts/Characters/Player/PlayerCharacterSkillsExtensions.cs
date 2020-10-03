namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using System;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class PlayerCharacterSkillsExtensions
    {
        public static SkillLevelData ServerAddSkillExperience<TProtoSkill>(this ICharacter character, double experience)
            where TProtoSkill : IProtoSkill, new()
        {
            return character.SharedGetSkills()
                            .ServerAddSkillExperience<TProtoSkill>(experience);
        }

        public static SkillLevelData ServerAddSkillExperience(
            this ICharacter character,
            IProtoSkill skill,
            double experience)
        {
            return character.SharedGetSkills()
                            .ServerAddSkillExperience(skill, experience);
        }

        public static SkillLevelData ServerDebugSetSkill<TProtoSkill>(this ICharacter character, byte level)
            where TProtoSkill : IProtoSkill, new()
        {
            return character.SharedGetSkills()
                            .ServerDebugSetSkill<TProtoSkill>(level);
        }

        public static SkillLevelData ServerDebugSetSkill(this ICharacter character, IProtoSkill skill, byte level)
        {
            return character.SharedGetSkills()
                            .ServerDebugSetSkill(skill, level);
        }

        public static SkillLevelData ServerDebugSetSkillExperience(
            this ICharacter character,
            IProtoSkill skill,
            double experience)
        {
            return character.SharedGetSkills()
                            .ServerDebugSetSkillExperience(skill, experience);
        }

        public static SkillLevelData SharedGetSkill(this ICharacter character, IProtoSkill skill)
        {
            return character.SharedGetSkills()
                            .SharedGetSkill(skill);
        }

        public static SkillLevelData SharedGetSkill<TProtoSkill>(this ICharacter character)
            where TProtoSkill : IProtoSkill, new()
        {
            return character.SharedGetSkills()
                            .SharedGetSkill<TProtoSkill>();
        }

        /// <summary>
        /// Please note: only player characters has skills. For NPC/Mobs this method will return null
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public static PlayerCharacterSkills SharedGetSkills(this ICharacter character)
        {
            if (!(character.ProtoCharacter is PlayerCharacter))
            {
                return null;
            }

            return PlayerCharacter.GetPrivateState(character).Skills;
        }

        public static bool SharedHasSkill<TProtoSkill>(this ICharacter character, byte level)
            where TProtoSkill : IProtoSkill, new()
        {
            return character.SharedGetSkills()
                            .SharedHasSkill<TProtoSkill>(level);
        }

        public static bool SharedHasSkill(this ICharacter character, IProtoSkill skill, byte level)
        {
            return character.SharedGetSkills()
                            .SharedHasSkill(skill, level);
        }

        public static bool SharedHasSkillFlag<TFlag>(this ICharacter character, TFlag flag)
            where TFlag : struct, Enum
        {
            var enumType = typeof(TFlag);
            var declaringType = enumType.DeclaringType;
            if (declaringType is null)
            {
                throw new Exception(
                    $"Cannot use this enum type for HasSkillFlag check - {enumType.FullName} - it should be created as the inner type for skill class");
            }

            if (Api.Shared.GetProtoEntityAbstract(declaringType) is ProtoSkill<TFlag> skill)
            {
                var data = character.SharedGetSkill(skill);
                return skill.SharedCheckFlagForLevel(flag, data.Level);
            }

            throw new Exception(
                $"Cannot use this enum type for HasSkillFlag check - {enumType.FullName} - it should be defined in the skill class.");
        }
    }
}