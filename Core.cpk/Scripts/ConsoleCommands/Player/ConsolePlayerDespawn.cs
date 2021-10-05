// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerDespawn : BaseConsoleCommand
    {
        public override string Description =>
            "Despawns the specified player's character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.despawn";

        public string Execute([CurrentCharacterIfNull] ICharacter player)
        {
            CharacterDespawnSystem.DespawnCharacter(player);
            return player + " despawned";
        }
    }
}