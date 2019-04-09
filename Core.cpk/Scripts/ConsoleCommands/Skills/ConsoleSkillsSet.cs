// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleSkillsSet : BaseConsoleCommand
    {
        public override string Description => "Set a given skill for a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "skills.set";

        public string Execute(IProtoSkill skill, byte level = 1, [CurrentCharacterIfNull] ICharacter player = null)
        {
            player.ServerDebugSetSkill(skill, level);
            return $"{player} skill {skill.Name} level set to {player.SharedGetSkill(skill).Level}.";
        }
    }
}