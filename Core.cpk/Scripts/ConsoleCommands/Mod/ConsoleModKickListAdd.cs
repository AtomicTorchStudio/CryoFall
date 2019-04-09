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
            "Kicks the player from the server for the defined amount of time.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "mod.kickList.add";

        public string Execute(ICharacter character, int minutes = 30)
        {
            if (character == null)
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

            ServerPlayerAccessSystem.Kick(character, minutes);
            return $"{character.Name} successfully kicked from the server for {minutes} minutes";
        }
    }
}