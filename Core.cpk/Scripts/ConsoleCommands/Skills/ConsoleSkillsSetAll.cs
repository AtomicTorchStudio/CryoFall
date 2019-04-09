// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Skills
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleSkillsSetAll : BaseConsoleCommand
    {
        public override string Description => "Set specific level for all skills of a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "skills.setAll";

        public string Execute(byte level = 100, [CurrentCharacterIfNull] ICharacter player = null)
        {
            foreach (var skill in Api.FindProtoEntities<IProtoSkill>())
            {
                level = player.ServerDebugSetSkill(skill, level).Level;

                if (level == skill.MaxLevel
                    || level == 0)
                {
                    continue;
                }

                // generate experience level random between current level and next level
                var maxExp = (int)(skill.GetExperienceForLevel((byte)(level + 1)) - skill.GetExperienceForLevel(level));
                var randomExp = Api.Random.Next(0, maxExp);

                var exp = skill.GetExperienceForLevel(level) + randomExp;

                // don't use AddSkillExperience as it will add LP too... which is not intended
                //player.ServerAddSkillExperience(skill, experience: randomExp);

                player.ServerDebugSetSkillExperience(skill, exp);
            }

            return $"{player} now has all skills set as:"
                   + Environment.NewLine
                   + ConsoleSkillsList.PrintSkills(player);
        }
    }
}