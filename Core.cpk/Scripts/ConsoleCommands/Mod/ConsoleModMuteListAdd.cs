// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleModMuteListAdd : BaseConsoleCommand
    {
        private static readonly int DefaultDuration = (int)TimeSpan.FromDays(30 * 3).TotalMinutes;

        public override string Alias => "mute";

        public override string Description =>
            "Mutes the player on the server for the defined amount of time.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "mod.muteList.add";

        public string Execute(ICharacter character, int? minutes = null)
        {
            if (character == null)
            {
                throw new Exception("The character name is not provided");
            }

            if (character == this.ExecutionContextCurrentCharacter)
            {
                throw new Exception("You cannot mute yourself");
            }

            if (minutes == null)
            {
                minutes = DefaultDuration;
            }

            if (minutes < 1)
            {
                throw new Exception($"Minutes must be in 1-{int.MaxValue} range");
            }

            ServerPlayerMuteSystem.Mute(character, minutes.Value);
            return string.Format("{0} successfully muted on the server for {1}",
                                 character.Name,
                                 ClientTimeFormatHelper.FormatTimeDuration(minutes.Value * 60.0));
        }
    }
}