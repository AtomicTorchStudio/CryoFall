namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleTechResetTechTree : BaseConsoleCommand
    {
        public override string Description => "Reset player's tech tree and refund all the LP.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "tech.resetTechTreeAndRefundLP";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();
            technologies.ServerResetTechTreeAndRefundLearningPoints();
            return null;
        }
    }
}