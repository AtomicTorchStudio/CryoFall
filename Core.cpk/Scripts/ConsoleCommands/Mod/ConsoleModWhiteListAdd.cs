// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;

    public class ConsoleModWhiteListAdd : BaseConsoleCommand
    {
        public override string Alias => "whiteListAdd";

        public override string Description =>
            "Adds a player name into the whitelist.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.whiteList.add";

        public string Execute(
            [CustomSuggestions(nameof(GetCharacterNameSuggestions))]
            string playerName)
        {
            if (ServerPlayerAccessSystem.SetWhiteListEntry(playerName, isEnabled: true))
            {
                return
                    $"{playerName} added to the whitelist (please don't forget to enable the whitelist if you want to use it and still didn't done it)";
            }

            return $"{playerName} is already in the whitelist";
        }

        private static IEnumerable<string> GetCharacterNameSuggestions(string startsWith)
        {
            return ParameterTypeCharacter.GetNameSuggestionsNonStrict(startsWith);
        }
    }
}