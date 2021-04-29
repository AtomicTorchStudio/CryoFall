// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.CharacterOrigins;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetOrigin : BaseConsoleCommand
    {
        public override string Description => "Change origin for a player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setOrigin";

        public string Execute(ProtoCharacterOrigin origin, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var privateState = PlayerCharacter.GetPrivateState(player);
            privateState.Origin = origin;
            privateState.SetFinalStatsCacheIsDirty();
            return player + " origin changed to " + origin.ShortId;
        }
    }
}