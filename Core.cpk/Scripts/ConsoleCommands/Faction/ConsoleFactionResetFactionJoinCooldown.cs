// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Faction
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleFactionResetFactionJoinCooldown : BaseConsoleCommand
    {
        public override string Description =>
            "Resets the faction leave join cooldown. Player can join any faction instantly.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "faction.resetFactionJoinCooldown";

        public string Execute([CurrentCharacterIfNull] ICharacter player)
        {
            PlayerCharacter.GetPrivateState(player).LastFactionLeaveTime = 0;
            return "Reset last faction leave time for " + player;
        }
    }
}