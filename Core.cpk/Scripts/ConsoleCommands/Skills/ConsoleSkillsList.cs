// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Skills
{
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleSkillsList : BaseConsoleCommand
    {
        public override string Description => "Print full list of skills of a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "skills.list";

        public static string PrintSkills(ICharacter player)
        {
            var skills = player.SharedGetSkills().Skills;
            var sb = new StringBuilder(player.ToString())
                     .Append(" total skills count: ")
                     .Append(skills.Count)
                     .AppendLine()
                     .Append("Skill levels:");

            foreach (var pair in skills)
            {
                var skill = pair.Key;
                var data = pair.Value;
                var expForNextLevel = data.ExperienceForNextLevel;

                sb.AppendLine()
                  .Append("   ")
                  .Append(skill.ShortId)
                  .Append(": level=")
                  .Append(data.Level)
                  .Append(", exp=")
                  .Append(data.Experience.ToString("F2"))
                  .Append("/")
                  .Append(expForNextLevel == double.MaxValue ? "<max reached>" : expForNextLevel.ToString("F0"));
            }

            return sb.ToString();
        }

        public string Execute([CurrentCharacterIfNull] ICharacter player)
        {
            return PrintSkills(player);
        }
    }
}