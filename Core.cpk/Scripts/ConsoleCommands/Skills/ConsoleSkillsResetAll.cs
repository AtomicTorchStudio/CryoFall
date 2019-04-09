// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleSkillsResetAll : BaseConsoleCommand
    {
        public override string Description => "Reset all the player skills.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "skills.resetAll";

        public string Execute([CurrentCharacterIfNull] ICharacter player)
        {
            var skills = player.SharedGetSkills();
            skills.ServerReset();
            return $"{player} skills reset.";
        }
    }
}