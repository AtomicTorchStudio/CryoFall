// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleDebugShowTechStatistics : BaseConsoleCommand
    {
        public override string Description => "Provides detailed statistics for all technology in the game.";

        // Available for both client and server, OP required to run this on server.
        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ClientAndServerOperatorOnly;

        public override string Name => "debug.showTechStatistics";

        public string Execute()
        {
            var listGroups = Api.FindProtoEntities<TechGroup>();
            var groupsByTier = listGroups.GroupBy(g => g.Tier);

            // calculating values
            var tierGroups = groupsByTier.ToDictionary(p => p.Key, p => p.Count());
            var tierGroupsPrice = groupsByTier.ToDictionary(
                p => p.Key,
                p => p.Sum(group => (int)group.LearningPointsPrice));
            var tierNodes = groupsByTier.ToDictionary(p => p.Key, p => p.Sum(group => group.AllNodes.Count));
            var tierNodesPrice = groupsByTier.ToDictionary(
                p => p.Key,
                p => p.Sum(gr => gr.AllNodes.Sum(node => (int)node.LearningPointsPrice)));

            // total statistics
            var totalGroupPrice = tierGroupsPrice.Sum(p => (int)p.Value);
            var totalNodePrice = tierNodesPrice.Sum(p => (int)p.Value);
            var totalPrice = totalGroupPrice + totalNodePrice;

            // build the output
            var sb = new StringBuilder();

            sb.AppendLine();
            foreach (var temp in tierGroups.OrderBy(i => i.Key))
            {
                sb.Append("Tier ").Append(temp.Key).Append(":");
                sb.AppendLine();
                sb.Append("   Groups - ").Append(temp.Value).AppendLine();
                sb.Append("   Groups unlock LP - ").Append(tierGroupsPrice[temp.Key]).AppendLine();
                sb.Append("   Nodes - ").Append(tierNodes[temp.Key]).AppendLine();
                sb.Append("   Nodes unlock LP - ").Append(tierNodesPrice[temp.Key]).AppendLine();
            }

            sb.Append("Total: ").AppendLine();
            sb.Append("   Groups LP - ").Append(totalGroupPrice).AppendLine();
            sb.Append("   Nodes LP - ").Append(totalNodePrice).AppendLine();
            sb.Append("   All LP - ").Append(totalPrice).AppendLine();

            return sb.ToString();
        }
    }
}