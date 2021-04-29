// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Faction
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleFactionResetFactionJoinCooldown : BaseConsoleCommand
    {
        public override string Description =>
            "Resets the faction leave join cooldown. Player can join any faction instantly.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "faction.resetFactionJoinCooldown";

        public string Execute([CurrentCharacterIfNull] ICharacter player)
        {
            PlayerCharacter.GetPrivateState(player).LastFactionLeaveTime = 0;

            using var tempListAllFactions = Api.Shared.GetTempList<ILogicObject>();
            GetProtoEntity<Faction>().GetAllGameObjects(tempListAllFactions.AsList());
            foreach (var faction in tempListAllFactions.AsList())
            {
                Faction.GetPrivateState(faction).ServerPlayerLeaveDateDictionary.Remove(player.Id);
            }

            return "Reset last faction leave time for " + player;
        }
    }
}