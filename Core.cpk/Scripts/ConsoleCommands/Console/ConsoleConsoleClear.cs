namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Console
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ConsoleConsoleClear : BaseConsoleCommand
    {
        public override string Alias => "clear";

        public override string Description => "Clears the console history.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.Client;

        public override string Name => "console.clear";

        public string Execute()
        {
            ConsoleControl.Instance.Clear();
            return null;
        }
    }
}