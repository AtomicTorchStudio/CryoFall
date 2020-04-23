// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;

    public class ConsoleModWhiteListRemove : BaseConsoleCommand
    {
        public override string Description =>
            "Removes a player name from the whitelist.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.whiteList.remove";

        public string Execute(
            [CustomSuggestions(nameof(GetCharacterNameSuggestions))]
            string playerName)
        {
            if (ServerPlayerAccessSystem.SetWhiteListEntry(playerName, isEnabled: false))
            {
                return $"{playerName} removed from the whitelist";
            }

            return $"{playerName} is not found in the whitelist";
        }

        private static IEnumerable<string> GetCharacterNameSuggestions(string startsWith)
        {
            return ServerPlayerAccessSystem.GetWhiteList();
        }
    }
}