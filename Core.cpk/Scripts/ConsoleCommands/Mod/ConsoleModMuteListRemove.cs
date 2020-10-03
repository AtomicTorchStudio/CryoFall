// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;

    public class ConsoleModMuteListRemove : BaseConsoleCommand
    {
        public override string Description =>
            "Un-mutes the player (remove from the temporary mute list on the server).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.muteList.remove";

        public string Execute(
            [CustomSuggestions(nameof(GetCharacterNameSuggestions))]
            string playerName)
        {
            var character = Server.Characters.GetPlayerCharacter(playerName);
            if (character is null)
            {
                throw new Exception("The character name is not provided");
            }

            if (ServerPlayerMuteSystem.Unmute(character))
            {
                return $"{character.Name} successfully un-muted";
            }

            return $"{character.Name} was not muted so no changes are done";
        }

        private static IEnumerable<string> GetCharacterNameSuggestions(string startsWith)
        {
            return ServerPlayerMuteSystem.GetMuteList();
        }
    }
}