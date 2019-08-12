// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.Editor.Console.Debug
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleFindNameCollisions : BaseConsoleCommand
    {
        public override string Description => "Finds prototypes which have the duplicate name.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ClientAndServerOperatorOnly;

        public override string Name => "debug.findNameCollisions";

        public string Execute()
        {
            var groups = Api.FindProtoEntities<IProtoEntity>()
                            .Where(e => !(e is TechNode) && !(e is Recipe))
                            .GroupBy(n => n.Name)
                            .OrderBy(g => g.Key)
                            .Where(g => g.Count() > 1)
                            .ToList();

            if (groups.Count == 0)
            {
                return "No name collisions found.";
            }

            return "Found prototypes with the same name:"
                   + Environment.NewLine
                   + groups.Select(g => (" * \"" + g.Key + "\":")
                                        + Environment.NewLine
                                        + g.Select(e => "   - " + e.Id)
                                           .GetJoinedString(Environment.NewLine))
                           .GetJoinedString(Environment.NewLine + Environment.NewLine);
        }
    }
}