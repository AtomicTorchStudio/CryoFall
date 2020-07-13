// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ConsoleTechSetGroupCompletion : BaseConsoleCommand
    {
        public override string Description =>
            "Add tech group and enable nodes to have desired percent of completion (value from 0 to 1, inclusive) to a player. This is useful when you need a particular tech group to have some nodes, but not all.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "tech.setGroupCompletion";

        public string Execute(
            TechGroup techGroup,
            double completionPercent,
            [CurrentCharacterIfNull] ICharacter player = null)
        {
            completionPercent = MathHelper.Clamp(completionPercent, 0, 1);

            var technologies = player.SharedGetTechnologies();
            technologies.ServerAddGroup(techGroup);

            foreach (var node in technologies.Nodes
                                             .Where(n => n.Group == techGroup)
                                             .ToList())
            {
                technologies.ServerRemoveNode(node);
            }

            var nodesToAddCount = (int)Math.Round(
                completionPercent * techGroup.Nodes.Count,
                MidpointRounding.AwayFromZero);
            if (nodesToAddCount <= 0)
            {
                return
                    $"{player} tech group {techGroup.NameWithTierName} cannot be added - there are not enough nodes for {completionPercent:F2} completion percent.";
            }

            var nodesToAdd = techGroup.Nodes;
            var nodeToAddIndex = 0;

            while (nodesToAddCount-- > 0)
            {
                var nodeToAdd = nodesToAdd[nodeToAddIndex++];
                technologies.ServerAddNode(nodeToAdd);
            }

            return $"{player} tech group {techGroup.NameWithTierName} added.";
        }
    }
}