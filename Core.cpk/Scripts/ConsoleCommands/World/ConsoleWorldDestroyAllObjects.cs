// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.World
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;

    [PrototypeIgnoredForNetwork]
    public class ConsoleWorldDestroyAllObjects : BaseConsoleCommand
    {
        public override string Description =>
            @"Destroys all static world objects of the specific prototype.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "world.destroyAll";

        public string Execute(IProtoStaticWorldObject protoObject)
        {
            var list = Server.World
                             .GetGameObjectsOfProto<IStaticWorldObject, IProtoStaticWorldObject>(protoObject)
                             .ToList();

            foreach (var obj in list)
            {
                Server.World.DestroyObject(obj);
            }

            return $"Destroyed {list.Count} objects of type: {protoObject.ShortId}";
        }
    }
}