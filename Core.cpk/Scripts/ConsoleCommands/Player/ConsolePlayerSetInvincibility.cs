// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.CharacterInvincibility;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetInvincibility : BaseConsoleCommand
    {
        public override string Alias => "god";

        public override string Description =>
            "Sets invincibility to a player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setInvincibility";

        public string Execute(bool isEnabled = true, [CurrentCharacterIfNull] ICharacter character = null)
        {
            if (isEnabled)
            {
                CharacterInvincibilitySystem.ServerAdd(character);
            }
            else
            {
                CharacterInvincibilitySystem.ServerRemove(character);
            }

            return $"{character} invincibility mode is {(isEnabled ? "on" : "off")}";
        }
    }
}