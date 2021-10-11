namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerAutosave;

    public class ConsoleDebugSave : BaseConsoleCommand
    {
        public override string Description =>
            "Invoke server autosave. You should not normally use it—the server should automatically do a snapshot as it's configured. When the server is shutting down it should also perform an autosave.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.save";

        public string Execute()
        {
            ServerAutosaveSystem.Instance.ServerExecuteAutosave();
            return "Autosave scheduled and will run shortly";
        }
    }
}