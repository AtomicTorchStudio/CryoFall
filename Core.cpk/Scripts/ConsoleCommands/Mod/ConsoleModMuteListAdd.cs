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

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.muteList.add";

        public string Execute(ICharacter character, int? minutes = null)
        {
            if (character is null)
            {
                throw new Exception("The character name is not provided");
            }

            minutes ??= DefaultDuration;
            if (minutes < 0)
            {
                throw new Exception($"Minutes must be in 0-{int.MaxValue} range");
            }

            if (minutes.Value == 0)
            {
                // un-mute
                if (ServerPlayerMuteSystem.Unmute(character))
                {
                    return $"{character.Name} successfully un-muted";
                }

                return $"{character.Name} was not muted so no changes are done";
            }

            if (character == this.ExecutionContextCurrentCharacter)
            {
                throw new Exception("You cannot mute yourself");
            }

            ServerPlayerMuteSystem.Mute(character, minutes.Value);
            return string.Format("{0} successfully muted on the server for {1}",
                                 character.Name,
                                 ClientTimeFormatHelper.FormatTimeDuration(minutes.Value * 60.0));
        }
    }
}