// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;

    public class ConsoleModKickListRemove : BaseConsoleCommand
    {
        public override string Description =>
            "Un-kicks the player (remove from the temporary kick list on the server).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.kickList.remove";

        public string Execute(
            [CustomSuggestions(nameof(GetCharacterNameSuggestions))]
            string playerName)
        {
            var character = Server.Characters.GetPlayerCharacter(playerName);
            if (character == null)
            {
                throw new Exception("The character name is not provided");
            }

            if (ServerPlayerAccessSystem.Unkick(character))
            {
                return $"{character.Name} successfully un-kicked";
            }

            return $"{character.Name} was not kicked so no changes are done";
        }

        private static IEnumerable<string> GetCharacterNameSuggestions(string startsWith)
        {
            return ServerPlayerAccessSystem.GetKickList();
        }
    }
}