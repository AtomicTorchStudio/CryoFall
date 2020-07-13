namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleResetLandClaimOwner : BaseConsoleCommand
    {
        public override string Description => "Resets the land claim founder to null and resets the access list.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.resetLandClaimOwner";

        public string Execute(ushort x, ushort y)
        {
            // reset to special name
            return ConsoleSetLandClaimOwner.ServerSetOwner(x, y, "[SERVER RESET NAME]");
        }

        public string Execute([CurrentCharacterIfNull] ICharacter nearCharacter)
        {
            return this.Execute(nearCharacter.TilePosition.X,
                                nearCharacter.TilePosition.Y);
        }
    }
}