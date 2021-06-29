namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleDebugTestRaidblock : BaseConsoleCommand
    {
        public override string Description => "Enable raid block for the base.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.testRaidblock";

        public string Execute(ushort x, ushort y, double durationMultiplier = 1.0)
        {
            durationMultiplier = MathHelper.Clamp(durationMultiplier, 0.01, 1);
            LandClaimSystem.ServerOnRaid(new RectangleInt(x, y, 1, 1),
                                         byCharacter: null,
                                         isStructureDestroyed: true,
                                         forceEvenIfNoCharacter: true,
                                         durationMultiplier);
            return null;
        }

        public string Execute(double durationMultiplier = 1.0, [CurrentCharacterIfNull] ICharacter character = null)
        {
            return this.Execute(character.TilePosition.X,
                                character.TilePosition.Y,
                                durationMultiplier);
        }
    }
}