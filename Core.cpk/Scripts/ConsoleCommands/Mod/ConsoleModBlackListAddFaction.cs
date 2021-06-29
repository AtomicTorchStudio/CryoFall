// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;

    public class ConsoleModBlackListAddFaction : BaseConsoleCommand
    {
        public override string Alias => "banFaction";

        public override string Description =>
            @"Adds members of the specified faction into the blacklist.
              Players from this list cannot connect to the game server
              (unless a player is an admin or moderator).
              Please note: there is also a whitelist that is intended
              to work in an opposite way by allowing access only to those players
              that are listed in a whitelist while everyone else is not allowed.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.blackList.addFaction";

        public string Execute(
            [CustomSuggestions(nameof(GetClanTagSuggestions))]
            string clanTag)
        {
            var faction = FactionSystem.ServerGetFactionByClanTag(clanTag);

            var bannedCount = 0;
            foreach (var playerName in FactionSystem.ServerGetFactionMemberNames(faction))
            {
                var character = Server.Characters.GetPlayerCharacter(playerName);
                if (character == this.ExecutionContextCurrentCharacter)
                {
                    continue;
                }

                if (ServerPlayerAccessSystem.SetBlackListEntry(playerName, isEnabled: true))
                {
                    bannedCount++;
                }
            }

            if (bannedCount == 0)
            {
                return $"No need to add anyone from the faction [{clanTag}] to the blacklist (already added)";
            }

            return $"{bannedCount} player(s) of the faction [{clanTag}] were added to the blacklist";
        }

        private static IEnumerable<string> GetClanTagSuggestions(string startsWith)
        {
            return FactionSystem.ServerFindClanTags(startsWith);
        }
    }
}