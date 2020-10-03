// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterInvincibility;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsolePlayerKillMe : BaseConsoleCommand
    {
        private const double CooldownSeconds = 5 * 60;

        private readonly Dictionary<ICharacter, double> characterLastKillTime
            = Api.IsServer ? new Dictionary<ICharacter, double>() : null;

        public override string Alias => "killMe";

        public override string Description =>
            "Kills you. Use when you desperately need to get rid of yourself and respawn."
            + " You cannot kill yourself more often than once in 5 minutes.";

        // this command was changed to server operator only
        // the game now has an option to unstuck player directly from the inventory window
        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.killMe";

        public string Execute()
        {
            var player = this.ExecutionContextCurrentCharacter;
            if (player is null)
            {
                return "This command cannot be executed directly from the server console";
            }

            var stats = player.GetPublicState<PlayerCharacterPublicState>()
                              .CurrentStatsExtended;

            if (stats.HealthCurrent <= 0)
            {
                return "You're already dead.";
            }

            // ensure character is not invincible
            CharacterInvincibilitySystem.ServerRemove(player);

            if (!ServerOperatorSystem.SharedIsOperator(player))
            {
                // check cooldown
                var time = Server.Game.FrameTime;
                if (this.characterLastKillTime.TryGetValue(player, out var lastKillTime)
                    && time < lastKillTime + CooldownSeconds)
                {
                    return string.Format(
                        "You've already killed yourself recently... please wait for cooldown: {0} remaining.",
                        ClientTimeFormatHelper.FormatTimeDuration(lastKillTime + CooldownSeconds - time));
                }

                this.characterLastKillTime[player] = time;
            }

            stats.ServerSetHealthCurrent(0);
            return "You've killed yourself.";
        }
    }
}