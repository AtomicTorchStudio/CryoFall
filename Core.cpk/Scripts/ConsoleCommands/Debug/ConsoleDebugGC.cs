namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleDebugGC : BaseConsoleCommand
    {
        public override string Alias => "gc";

        public override string Description => "Invoke GC (garbage collection).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ClientAndServerOperatorOnly;

        public override string Name => "debug.gc";

        public string Execute()
        {
            Api.Shared.ForceGCCollect(out var memoryBefore, out var memoryAfter);
            return
                $"GC completed. Memory use: {memoryBefore / (1024d * 1024d):F1}MB -> {memoryAfter / (1024d * 1024d):F1}MB";
        }
    }
}