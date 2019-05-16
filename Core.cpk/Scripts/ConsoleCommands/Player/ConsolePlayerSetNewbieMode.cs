namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetNewbieMode : BaseConsoleCommand
    {
        public override string Description
            => "Toggles newbie protection.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setNewbieMode";

        public string Execute(bool isEnabled, [CurrentCharacterIfNull] ICharacter character)
        {
            if (isEnabled)
            {
                NewbieProtectionSystem.ServerRegisterNewbie(character);
            }
            else
            {
                NewbieProtectionSystem.ServerDisableNewbieProtection(character);
            }

            return character
                   + " is now "
                   + (NewbieProtectionSystem.SharedIsNewbie(character)
                          ? "under newbie protection."
                          : "without newbie protection.");
        }
    }
}