namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleDebugResetRaidblock : BaseConsoleCommand
    {
        public override string Description => "Resets server raid block for the base.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.resetRaidblock";

        public string Execute(ushort x, ushort y)
        {
            LandClaimSystem.ServerResetRaidblock(new RectangleInt(x, y, 1, 1));
            return null;
        }

        public string Execute([CurrentCharacterIfNull] ICharacter character)
        {
            return this.Execute(character.TilePosition.X,
                                character.TilePosition.Y);
        }
    }
}