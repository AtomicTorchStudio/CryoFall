namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;

    public class ConsoleModKickListAddFaction : BaseConsoleCommand
    {
        public override string Alias => "kickFaction";

        public override string Description =>
            @"Kicks members of the specified faction from the server for the specified amount of time.
              If you want to add a kick reason, you can do this by writing it in the ""quotes""
              right after the number of minutes.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.kickList.addFaction";

        public string Execute(
            [CustomSuggestions(nameof(GetClanTagSuggestions))]
            string clanTag,
            int minutes = 30,
            string kickMessageInQuotes = null)
        {
            var faction = FactionSystem.ServerGetFactionByClanTag(clanTag);

            if (minutes < 1
                || minutes > 10000)
            {
                throw new Exception("Minutes must be in 1-10000 range");
            }

            var kickedCount = 0;
            foreach (var playerName in FactionSystem.ServerGetFactionMemberNames(faction))
            {
                var character = Server.Characters.GetPlayerCharacter(playerName);
                if (character == this.ExecutionContextCurrentCharacter)
                {
                    continue;
                }

                ServerPlayerAccessSystem.Kick(character, minutes, kickMessageInQuotes);
                kickedCount++;
            }

            if (kickedCount == 0)
            {
                return $"Cannot kick anyone from the faction [{clanTag}]";
            }

            var result =
                $"{kickedCount} player(s) of the faction [{clanTag}] successfully kicked from the server for {minutes} minutes";
            if (!string.IsNullOrEmpty(kickMessageInQuotes))
            {
                result += " with a following message:"
                          + Environment.NewLine
                          + kickMessageInQuotes;
            }

            return result;
        }

        private static IEnumerable<string> GetClanTagSuggestions(string startsWith)
        {
            return FactionSystem.ServerFindClanTags(startsWith);
        }
    }
}