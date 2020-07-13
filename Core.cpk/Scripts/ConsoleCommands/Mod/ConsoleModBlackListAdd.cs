// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;

    public class ConsoleModBlackListAdd : BaseConsoleCommand
    {
        public override string Alias => "ban";

        public override string Description =>
            @"Adds a player name into the blacklist.
              Players from this list cannot connect to the game server
              (unless a player is an admin or moderator).
              Please note: there is also a whitelist that is intended
              to work in an opposite way by allowing access only to those players
              that are listed in a whitelist while everyone else is not allowed.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.blackList.add";

        public string Execute(
            [CustomSuggestions(nameof(GetCharacterNameSuggestions))]
            string playerName)
        {
            if (ServerPlayerAccessSystem.SetBlackListEntry(playerName, isEnabled: true))
            {
                return $"{playerName} added to the blacklist";
            }

            return $"{playerName} is already in the blacklist";
        }

        private static IEnumerable<string> GetCharacterNameSuggestions(string startsWith)
        {
            return ParameterTypeCharacter.GetNameSuggestionsNonStrict(startsWith);
        }
    }
}