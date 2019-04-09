// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;

    public class ConsoleModBlackListRemove : BaseConsoleCommand
    {
        public override string Description =>
            "Removes a player name from the blacklist.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "mod.blackList.remove";

        public string Execute(
            [CustomSuggestions(nameof(GetCharacterNameSuggestions))]
            string playerName)
        {
            if (ServerPlayerAccessSystem.SetBlackListEntry(playerName, isEnabled: false))
            {
                return $"{playerName} removed from the blacklist";
            }

            return $"{playerName} is not found in the blacklist";
        }

        private static IEnumerable<string> GetCharacterNameSuggestions(string startsWith)
        {
            return ServerPlayerAccessSystem.GetBlackList();
        }
    }
}