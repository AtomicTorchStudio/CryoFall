// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerModerator;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleAdminModeratorAdd : BaseConsoleCommand
    {
        public override string Alias => "moderatorAdd";

        public override string Description
            => "Adds server moderator.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.moderator.add";

        public string Execute([CurrentCharacterIfNull] ICharacter character)
        {
            if (ServerModeratorSystem.ServerIsModerator(character.Name))
            {
                return character.Name + " is already a server moderator";
            }

            ServerModeratorSystem.ServerAdd(character);
            return character.Name + " added to the server moderators list";
        }
    }
}