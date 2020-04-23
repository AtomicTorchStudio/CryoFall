// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleAdminSpawnAll : BaseConsoleCommand
    {
        public override string Description =>
            @"Executes all spawn scripts.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.spawnAll";

        public string Execute()
        {
            foreach (var protoZone in Api.FindProtoEntities<IProtoZone>())
            {
                ConsoleAdminSpawnZone.ExecuteZoneScripts(protoZone);
            }

            return "All spawn scripts executed";
        }
    }
}