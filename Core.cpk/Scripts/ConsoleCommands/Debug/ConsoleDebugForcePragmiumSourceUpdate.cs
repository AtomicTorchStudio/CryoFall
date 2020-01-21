// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    // This command is added for debugging the pragmium source mineral internal spawn script for nodes and guardian creatures.
    public class ConsoleDebugForcePragmiumSourceUpdate : BaseConsoleCommand
    {
        public override string Description =>
            "Forces update of all pragmium source minerals in the game. They will attempt to spawn nodes and guardian creatures.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.forcePragmiumSourceUpdate";

        public string Execute()
        {
            var proto = Api.GetProtoEntity<ObjectMineralPragmiumSource>();
            var allPragmiumSources =
                Server.World.GetGameObjectsOfProto<IStaticWorldObject, ObjectMineralPragmiumSource>();

            var count = 0;
            foreach (var worldObject in Api.Shared.WrapInTempList(allPragmiumSources).EnumerateAndReturn())
            {
                proto.ServerForceUpdate(worldObject, deltaTime: 0);
                count++;
            }

            return "Pragmium sources updated. Total pragmium source objects: " + count;
        }
    }
}