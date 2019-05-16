namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data.TreeLayout;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelTechTreeControl : BaseViewModel
    {
        public const bool IsTestMode = false;

        private readonly BaseCommand commandOnNodeSelect = new ActionCommandWithParameter(ExecuteCommandOnNodeSelect);

        private readonly double nodeHeight;

        private List<BaseViewModel> items;

        private List<ViewModelTechNode> nodes;

        private TechGroup techGroup;

        public ViewModelTechTreeControl(
            double nodeSize,
            double nodeHorizontalMarginTotal,
            double nodeVerticalSpacing,
            double nodeLinkArrowHeight)
        {
            this.nodeHeight = nodeSize;
            this.NodeWidth = nodeSize + nodeHorizontalMarginTotal;
            this.NodeVerticalSpacing = nodeVerticalSpacing;
            this.HalfNodeVerticalSpacing = nodeVerticalSpacing / 2;
            this.NodeLevelHeight = nodeSize + nodeVerticalSpacing;
            this.NodeLinkArrowHeight = nodeLinkArrowHeight;
        }

        public double HalfNodeVerticalSpacing { get; }

        public List<BaseViewModel> Items
        {
            get => this.items;
            private set
            {
                var oldDataItems = this.items;
                this.items = value;
                if (oldDataItems != null)
                {
                    this.DisposeCollection(oldDataItems);
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public double NodeLevelHeight { get; }

        public double NodeLinkArrowHeight { get; }

        public List<ViewModelTechNode> Nodes
        {
            get => this.nodes;
            private set
            {
                // ensure that the nodes are sorted by hierarchy level
                value.SortBy(n => n.HierarchyLevel);
                this.nodes = value;

                this.CalculateTreeSize();

                var dataItems = new List<BaseViewModel>(capacity: this.nodes.Count * 2);
                this.AddLinks(dataItems);
                dataItems.AddRange(this.nodes);
                this.Items = dataItems;

                this.NotifyThisPropertyChanged();
            }
        }

        /// <summary>
        /// Vertical distance between two neighbor hierarchy levels.
        /// </summary>
        public double NodeVerticalSpacing { get; }

        /// <summary>
        /// Node width (including margin)
        /// </summary>
        public double NodeWidth { get; }

        public TechGroup TechGroup
        {
            get => this.techGroup;
            set
            {
                if (this.techGroup == value)
                {
                    return;
                }

                this.techGroup = value;

                if (this.techGroup == null
                    || this.techGroup.RootNodes.Count == 0)
                {
                    if (this.techGroup != null
                        && this.techGroup.RootNodes.Count == 0)
                    {
                        Api.Logger.Error($"Incorrect tech group \"{this.techGroup.Id}\" - no nodes");
                    }

                    this.Nodes = new List<ViewModelTechNode>(0);
                    return;
                }

                var newNodes = IsTestMode
                                   ? this.BuildTestTree()
                                   : this.BuildTree(this.techGroup);

                var roots = newNodes.Where(n => n.ParentNode == null).ToList();
                new TreeLayoutHelper<ViewModelTechNode>(
                        estimatedTreeMaxDepth: newNodes.Max(n => n.HierarchyLevel))
                    .DoLayout(roots);

                this.Nodes = newNodes;
            }
        }

        public double TreeHeight { get; private set; } = 1024;

        public double TreeWidth { get; private set; } = 1024;

        private static void ExecuteCommandOnNodeSelect(object obj)
        {
            var techNode = (ViewModelTechNode)obj;
            if (!techNode.CanUnlock)
            {
                return;
            }

            DialogWindow.ShowDialog(
                CoreStrings.QuestionAreYouSure,
                new TextBlock()
                {
                    Text = string.Format(ViewModelWindowTechnologies.DialogDoYouWantToResearch, techNode.Title),
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 400
                },
                okText: CoreStrings.Yes,
                cancelText: CoreStrings.No,
                okAction: () => TechnologiesSystem.ClientUnlockNode(techNode.TechNode),
                cancelAction: () => { });
        }

        /// <summary>
        /// Create link view models from children node to its parent node.
        /// </summary>
        /// <param name="result"></param>
        private void AddLinks(List<BaseViewModel> result)
        {
            foreach (var techNode in this.nodes)
            {
                if (techNode.ParentNode == null)
                {
                    continue;
                }

                var link = new ViewModelTechNodeLink(
                    techNode,
                    this.NodeWidth,
                    this.nodeHeight,
                    this.HalfNodeVerticalSpacing,
                    this.NodeLinkArrowHeight);
                result.Add(link);
            }
        }

        private List<ViewModelTechNode> BuildTestTree()
        {
            var dataNodes = TestNodesProvider.BuildTestTree();
            var vmNodes = new Dictionary<TestNodesProvider.TestNode, ViewModelTechNode>();
            foreach (var node in dataNodes)
            {
                vmNodes.Add(
                    node,
                    new ViewModelTechNode(
                        node.BuildFullName(),
                        node.Level,
                        this,
                        node.IsUnlocked,
                        canUnlock: node.Parent != null && node.Parent.IsUnlocked));
            }

            foreach (var pair in vmNodes)
            {
                var parent = pair.Key.Parent;
                if (parent != null)
                {
                    pair.Value.SetParentNode(vmNodes[parent]);
                }
            }

            return vmNodes.Values.ToList();
        }

        private List<ViewModelTechNode> BuildTree(TechGroup techGroup)
        {
            var nodes = new List<ViewModelTechNode>();
            if (techGroup == null)
            {
                return nodes;
            }

            foreach (var techNode in techGroup.Nodes)
            {
                nodes.Add(new ViewModelTechNode(techNode, this, this.commandOnNodeSelect));
            }

            // establish links
            foreach (var viewModelTechNode in nodes)
            {
                var requiredNode = viewModelTechNode.TechNode.RequiredNode;
                if (requiredNode == null)
                {
                    // root node
                    continue;
                }

                foreach (var otherViewModel in nodes)
                {
                    if (otherViewModel.TechNode == requiredNode)
                    {
                        viewModelTechNode.SetParentNode(otherViewModel);
                    }
                }
            }

            return nodes;
        }

        private void CalculateTreeSize()
        {
            if (this.nodes.Count == 0)
            {
                // no nodes (closed mode)
                return;
            }

            double treeWidth = 0, treeHeight = 0;
            foreach (var node in this.nodes)
            {
                if (treeWidth < node.PositionX)
                {
                    treeWidth = node.PositionX;
                }

                if (treeHeight < node.PositionY)
                {
                    treeHeight = node.PositionY;
                }
            }

            // add one node size
            // (because our coordinates are starting at bottom-left corner and nodes drawn in top-right direction)
            treeWidth += this.NodeWidth;
            treeHeight += this.nodeHeight;

            this.TreeWidth = treeWidth;
            this.TreeHeight = treeHeight;

            //Api.Logger.WriteDev($"Tree size: {this.TreeWidth:F0}*{this.TreeHeight:F0}");
        }
    }
}