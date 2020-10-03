// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerModerator;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleAdminModeratorRemove : BaseConsoleCommand
    {
        public override string Alias => "moderatorRemove";

        public override string Description
            => "Removes server moderator.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.moderator.remove";

        public string Execute([CurrentCharacterIfNull] ICharacter character)
        {
            if (!ServerModeratorSystem.ServerIsModerator(character.Name))
            {
                return character.Name + " is not a server moderator";
            }

            ServerModeratorSystem.ServerRemove(character);
            return character.Name + " removed from the server moderators list";
        }
    }
}