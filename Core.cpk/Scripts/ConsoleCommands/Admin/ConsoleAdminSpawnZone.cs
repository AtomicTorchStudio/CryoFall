// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleAdminSpawnZone : BaseConsoleCommand
    {
        public override string Description =>
            @"Executes all spawn scripts for the specified zone.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.spawnZone";

        public static void ExecuteZoneScripts(
            IProtoZone protoZone,
            bool isInitialSpawn = false)
        {
            var serverZoneInstance = protoZone.ServerZoneInstance;
            if (protoZone.AttachedScripts.Count == 0)
            {
                // No scripts attached to this zone
                return;
            }

            var trigger = isInitialSpawn
                              ? (ProtoTrigger)Api.GetProtoEntity<TriggerWorldInit>()
                              : (ProtoTrigger)Api.GetProtoEntity<TriggerTimeInterval>();

            using var tempList = Api.Shared.GetTempList<IZoneScriptConfig>();
            tempList.AddRange(protoZone.AttachedScripts);

            foreach (var script in tempList.AsList())
            {
                script.ServerInvoke(trigger, serverZoneInstance);
            }
        }

        public string Execute(IProtoZone protoZone)
        {
            ExecuteZoneScripts(protoZone);
            return $"All spawn scripts for {protoZone} executed";
        }
    }
}