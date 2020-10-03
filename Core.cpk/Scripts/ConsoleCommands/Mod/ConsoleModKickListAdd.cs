// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleModKickListAdd : BaseConsoleCommand
    {
        public override string Alias => "kick";

        public override string Description =>
            "Kicks the player from the server for the defined amount of time. If you want to add a kick reason, you can do this by writing it in the \"quotes\" right after the number of minutes.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.kickList.add";

        public string Execute(ICharacter character, int minutes = 30, string kickMessageInQuotes = null)
        {
            if (character is null)
            {
                throw new Exception("The character name is not provided");
            }

            if (character == this.ExecutionContextCurrentCharacter)
            {
                throw new Exception("You cannot kick yourself");
            }

            if (minutes < 1
                || minutes > 10000)
            {
                throw new Exception("Minutes must be in 1-10000 range");
            }

            ServerPlayerAccessSystem.Kick(character, minutes, kickMessageInQuotes);

            var result = $"{character.Name} successfully kicked from the server for {minutes} minutes";
            if (!string.IsNullOrEmpty(kickMessageInQuotes))
            {
                result += " with a following message:"
                          + Environment.NewLine
                          + kickMessageInQuotes;
            }

            return result;
        }
    }
}