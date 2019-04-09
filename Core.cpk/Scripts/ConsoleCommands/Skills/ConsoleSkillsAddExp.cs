// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleSkillsAddExp : BaseConsoleCommand
    {
        public override string Description => "Add skill experience for a specified skill.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "skills.addExp";

        public string Execute(IProtoSkill skill, double exp, [CurrentCharacterIfNull] ICharacter player = null)
        {
            player.ServerAddSkillExperience(skill, exp);
            return string.Format("{0} skill {1} experience added. Current skill level: {2}.",
                                 player,
                                 skill.Name,
                                 player.SharedGetSkill(skill).Level);
        }
    }
}