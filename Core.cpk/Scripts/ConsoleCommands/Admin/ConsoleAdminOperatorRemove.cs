// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleAdminOperatorRemove : BaseConsoleCommand
    {
        public override string Alias => "opRemove";

        public override string Description
            => "Removes server operator.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.operator.remove";

        public string Execute([CurrentCharacterIfNull] ICharacter character)
        {
            if (!ServerOperatorSystem.ServerIsOperator(character.Name))
            {
                return character.Name + " is not a server operator";
            }

            ServerOperatorSystem.ServerRemove(character);
            return character.Name + " removed from the server operators list";
        }
    }
}