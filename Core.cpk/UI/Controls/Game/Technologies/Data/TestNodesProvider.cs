namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    internal class TestNodesProvider
    {
        private const int FirstLevelNodesCount = 1;

        private const int MaxLevel = 5;

        private const int NodeMaxChildrenCount = 2;

        private const int NodeMinChildrenCount = 1;

        internal static List<TestNode> BuildTestTree()
        {
            var dataNodes = new List<TestNode>();

            RandomHelper.SetSeedRandom();
            GenerateTestNodes(dataNodes, parentNode: null, level: 0, maxLevel: MaxLevel, isUnlocked: true);

            Api.Logger.Important("Test nodes generated for tech tree. Total nodes: " + dataNodes.Count);

            return dataNodes;
        }

        private static void GenerateTestNodes(
            List<TestNode> testNodes,
            TestNode parentNode,
            int level,
            int maxLevel,
            bool isUnlocked)
        {
            var nextLevel = level + 1;
            var isLastLevel = nextLevel > maxLevel;

            var nodesCount = RandomHelper.Next(
                isLastLevel ? 0 : NodeMinChildrenCount,
                NodeMaxChildrenCount + 1);
            if (level == 0)
            {
                nodesCount = FirstLevelNodesCount;
            }

            for (var i = 0; i < nodesCount; i++)
            {
                var node = new TestNode
                {
                    Parent = parentNode,
                    Level = level,
                    LocalName = ((char)('A' + (char)i)).ToString(),
                    IsUnlocked = isUnlocked
                };

                testNodes.Add(node);

                if (!isLastLevel)
                {
                    GenerateTestNodes(
                        testNodes,
                        node,
                        nextLevel,
                        maxLevel,
                        isUnlocked && RandomHelper.Next(0, maxLevel) >= level);
                }
            }
        }

        internal class TestNode
        {
            public bool IsUnlocked;

            public int Level;

            public string LocalName;

            public TestNode Parent;

            public string BuildFullName()
            {
                var result = new StringBuilder();
                result.Append(this.LocalName);

                var parent = this.Parent;
                while (parent != null)
                {
                    result.Append(parent.LocalName);
                    parent = parent.Parent;
                }

                var arr = result.ToString().ToCharArray();
                Array.Reverse(arr);
                return new string(arr);
            }
        }
    }
}