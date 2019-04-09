// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.GameEngine.Common.Helpers;

    // This command is added for debugging the too many simultaneous healthbars issue.
    // It might be also useful as it allows to instantly repair or destroy all the player buildings in the world!
    public class ConsoleDebugSetAllStructuresHP : BaseConsoleCommand
    {
        public override string Description =>
            "Set structures bar to % of the max value to all structures in the world.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.setAllStructuresHP";

        public string Execute(float hpPercent)
        {
            hpPercent = MathHelper.Clamp(hpPercent, 0, 1);
            var list = Server.World.FindStaticWorldObjectsOfProto<IProtoObjectStructure>().ToList();

            foreach (var worldObject in list)
            {
                var protoObjectStructure = ((IProtoObjectStructure)worldObject.ProtoStaticWorldObject);
                var publicState = worldObject.GetPublicState<StaticObjectPublicState>();
                var newStructurePoints = protoObjectStructure.SharedGetStructurePointsMax(worldObject) * hpPercent;
                publicState.StructurePointsCurrent = newStructurePoints;
                protoObjectStructure.ServerApplyDamage(worldObject, 0);
            }

            return $"Changed HP of {list.Count} objects to {hpPercent:F2} of their max structure points";
        }
    }
}