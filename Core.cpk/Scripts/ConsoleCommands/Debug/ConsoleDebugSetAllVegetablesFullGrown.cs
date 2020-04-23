// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.Systems.Console;

    public class ConsoleDebugSetAllVegetablesFullGrown : BaseConsoleCommand
    {
        public override string Description =>
            "Set all vegetation to full grown state.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.setAllVegetationFullGrown";

        public string Execute()
        {
            var allVegetation = Server.World.GetStaticWorldObjectsOfProto<IProtoObjectVegetation>();
            var count = 0;
            foreach (var staticWorldObject in allVegetation)
            {
                var proto = (IProtoObjectVegetation)staticWorldObject.ProtoStaticWorldObject;
                proto.ServerSetFullGrown(staticWorldObject);
                count++;
            }

            return $"Vegetation set full grown: total {count} objects";
        }
    }
}