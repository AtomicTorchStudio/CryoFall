namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data.TreeLayout
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Generic tree layout algorithm implementation.
    /// It will layout tree in aesthetically pleasant form. Based on Walker's algorithm.
    /// Very useful for displaying tech trees, skill trees, etc trees.
    /// It has very little dependencies - your tree node class need only to implement <see cref="ITreeNodeForLayout{T}" />.
    /// References:
    /// (canonical algorithm)
    /// http://collaboration.cmc.ec.gc.ca/science/rpn/biblio/ddj/Website/articles/CUJ/1991/9102/walker/walker.htm
    /// (actually based on) https://rachel53461.wordpress.com/2014/04/20/algorithm-for-drawing-trees/
    /// (very cryptic but useful) https://llimllib.github.io/pymag-trees/
    /// </summary>
    /// <typeparam name="T">Tree node data type (must implement ITreeNodeForLayout for self).</typeparam>
    public class TreeLayoutHelper<T>
        where T : class, ITreeNodeForLayout<T>
    {
        private readonly int estimatedTreeMaxDepth;

        private readonly double siblingNodePadding;

        /// <summary>
        /// Create instance of the tree layout helper.
        /// </summary>
        /// <param name="siblingNodePadding">Padding between nodes (in "node" units). Value of 1 equals to single node width.</param>
        /// <param name="estimatedTreeMaxDepth">
        /// Estimated max depth (set it as close as possible to the maximum depth of the tree -
        /// very useful for optimization).
        /// </param>
        public TreeLayoutHelper(double siblingNodePadding = 1, int estimatedTreeMaxDepth = 16)
        {
            this.siblingNodePadding = siblingNodePadding;
            this.estimatedTreeMaxDepth = estimatedTreeMaxDepth;
        }

        /// <summary>
        /// Layout only single root node.
        /// </summary>
        /// <param name="rootNode">Root node.</param>
        public void DoLayout(T rootNode)
        {
            var layoutRootNode = this.CreateLayoutNode(rootNode);
            this.DoLayout(layoutRootNode);
        }

        /// <summary>
        /// Layout multiple root nodes.
        /// </summary>
        /// <param name="rootNodes">List of root nodes.</param>
        public void DoLayout(IReadOnlyCollection<T> rootNodes)
        {
            var layoutRootNode = new TreeLayoutNode(null, rootNodes.Count);
            foreach (var node in rootNodes)
            {
                var layoutNode = this.CreateLayoutNode(node);
                layoutRootNode.Children.Add(layoutNode);
                layoutNode.Parent = layoutRootNode;
            }

            this.DoLayout(layoutRootNode);
        }

        /// <summary>
        /// Calculate final positions - apply parent X shift values to their children.
        /// </summary>
        private void CalculateFinalX(TreeLayoutNode node, double shiftX)
        {
            // offset this node on the shift value
            node.X += shiftX;

            // write result X position
            node.SetLayoutPositionX(node.X);

            if (node.Children.Count == 0)
            {
                return;
            }

            // process children
            // all the next children will be offset on the combined shift value
            shiftX += node.ChildrenShiftX;
            foreach (var child in node.Children)
            {
                this.CalculateFinalX(child, shiftX);
            }
        }

        /// <summary>
        /// Calculate initial X and children shift values.
        /// </summary>
        /// <param name="node">Current node.</param>
        /// <param name="previousNode">Previous (left sibling to current) node.</param>
        private void CalculateInitialX(TreeLayoutNode node, TreeLayoutNode previousNode)
        {
            var childrenCount = node.Children.Count;
            if (childrenCount > 0)
            {
                TreeLayoutNode previousChildNode = null;
                foreach (var childNode in node.Children)
                {
                    this.CalculateInitialX(childNode, previousChildNode);
                    previousChildNode = childNode;
                }
            }

            if (childrenCount == 0)
            {
                // no children
                if (previousNode is null)
                {
                    // first node in the row
                    node.X = 0;
                }
                else
                {
                    // take previous node X and add the padding distance
                    node.X = previousNode.X + this.siblingNodePadding;
                }

                return;
            }

            // has children
            double middleX;
            var leftChild = node.Children[0];

            if (childrenCount == 1)
            {
                // single children - middleX equals to the (only) child X
                middleX = leftChild.X;
            }
            else
            {
                // many children - calculate middle X
                var rightChild = node.Children[node.Children.Count - 1];
                middleX = (leftChild.X + rightChild.X) / 2;
            }

            if (previousNode is null)
            {
                // no left sibling
                node.X = middleX;
            }
            else
            {
                // has left sibling - offset accordingly
                node.X = previousNode.X + this.siblingNodePadding;
                node.ChildrenShiftX = node.X - middleX;

                // since subtrees can overlap, check for conflicts and shift tree right wherever needed
                this.FixConflicts(node);
            }
        }

        /// <summary>
        /// Calculate tree side contour (recursive).
        /// </summary>
        private void CalculateTreeContour(
            TreeLayoutNode node,
            List<double> contour,
            bool isLeftContour,
            int maxArrayIndex = int.MaxValue,
            int arrayLevel = 0,
            double shiftX = 0)
        {
            var currentNodeX = node.X + shiftX;

            if (arrayLevel < contour.Count)
            {
                var currentValue = contour[arrayLevel];
                contour[arrayLevel] = isLeftContour
                                          ? Math.Min(currentValue, currentNodeX)
                                          : Math.Max(currentValue, currentNodeX);
            }
            else
            {
                contour.Add(currentNodeX);
            }

            if (arrayLevel == maxArrayIndex
                || node.Children.Count == 0)
            {
                // no need (or cannot) traverse deeper
                return;
            }

            shiftX += node.ChildrenShiftX;
            foreach (var child in node.Children)
            {
                this.CalculateTreeContour(child, contour, isLeftContour, maxArrayIndex, arrayLevel + 1, shiftX);
            }
        }

        private TreeLayoutNode CreateLayoutNode(T rootNode)
        {
            var childrenCount = rootNode.Children.Count;
            var layoutNode = new TreeLayoutNode(rootNode, childrenCount);
            if (childrenCount == 0)
            {
                return layoutNode;
            }

            var layoutNodeChildren = layoutNode.Children;
            foreach (var childNode in rootNode.Children)
            {
                var childLayoutNode = this.CreateLayoutNode(childNode);
                childLayoutNode.Parent = layoutNode;
                layoutNodeChildren.Add(childLayoutNode);
            }

            return layoutNode;
        }

        /// <summary>
        /// Distribute nodes evenly between two sibling nodes.
        /// It will do nothing if there are no nodes between the two nodes.
        /// </summary>
        /// <param name="leftNode">Node on the left.</param>
        /// <param name="rightNode">Node on the right.</param>
        private void DistributeEvenlyNodesBetween(TreeLayoutNode leftNode, TreeLayoutNode rightNode)
        {
            var parentChildren = leftNode.Parent.Children;
            var leftIndex = parentChildren.IndexOf(leftNode);
            var rightIndex = parentChildren.IndexOf(rightNode);

            var countNodesBetween = rightIndex - leftIndex - 1;
            if (countNodesBetween < 1)
            {
                return;
            }

            var distanceBetweenNodes = (leftNode.X - rightNode.X)
                                       / (countNodesBetween + 1);

            var count = 1;
            for (var i = leftIndex + 1; i < rightIndex; i++, count++)
            {
                var middleNode = parentChildren[i];

                var desiredX = rightNode.X + distanceBetweenNodes * count;
                var offset = desiredX - middleNode.X;
                middleNode.X += offset;
                middleNode.ChildrenShiftX += offset;

                this.FixConflicts(middleNode);
            }
        }

        private void DoLayout(TreeLayoutNode layoutRootNode)
        {
            this.CalculateInitialX(layoutRootNode, previousNode: null);
            this.EnsureAllNodesInScreenBounds(layoutRootNode);
            this.CalculateFinalX(layoutRootNode, shiftX: 0);
        }

        /// <summary>
        /// Traverse the tree and ensure that every node is in the screen bounds (minimal X>=0).
        /// </summary>
        private void EnsureAllNodesInScreenBounds(TreeLayoutNode rootNode)
        {
            // calculate left contour
            var contour = new List<double>(capacity: this.estimatedTreeMaxDepth);
            this.CalculateTreeContour(rootNode, contour, isLeftContour: true);

            // calculate min X position in contour
            var minX = contour.Min();

            // shift root node to keep it in bounds
            rootNode.X -= minX;
            rootNode.ChildrenShiftX -= minX;
        }

        /// <summary>
        /// Check and resolve conflicts with all the left-side sibling trees.
        /// This method will check collisions of current node tree with all the sibling node trees on the left side starting from
        /// the most left node.
        /// The implementation will calculate left contour of the provided node and compare it with the right contours of the
        /// sibling nodes.
        /// </summary>
        /// <param name="node">Current node (it will be shifted when needed).</param>
        private void FixConflicts(TreeLayoutNode node)
        {
            var siblingNode = node.GetLeftMostSibling();
            if (siblingNode == node)
            {
                return;
            }

            var minDistance = this.siblingNodePadding;
            var nodeLeftContour = new List<double>(capacity: this.estimatedTreeMaxDepth);
            var nodeLeftContourMaxLevel = 0;
            var isNeedRecalculateNodeLeftContour = true;
            var siblingNodeRightContour = new List<double>(capacity: this.estimatedTreeMaxDepth);

            do
            {
                if (isNeedRecalculateNodeLeftContour)
                {
                    // calculate current node left contour
                    isNeedRecalculateNodeLeftContour = false;
                    nodeLeftContour.Clear();
                    this.CalculateTreeContour(node, nodeLeftContour, isLeftContour: true);
                    nodeLeftContourMaxLevel = nodeLeftContour.Count - 1;
                }

                if (siblingNodeRightContour.Count > 0)
                {
                    siblingNodeRightContour.Clear();
                }

                // calculate right contour of the sibling tree
                this.CalculateTreeContour(
                    siblingNode,
                    siblingNodeRightContour,
                    isLeftContour: false,
                    maxArrayIndex: nodeLeftContourMaxLevel);
                var siblingNodeRightContourMaxLevel = siblingNodeRightContour.Count - 1;
                var maxLevel = Math.Min(nodeLeftContourMaxLevel, siblingNodeRightContourMaxLevel);

                // compare contours to find shift value
                // (separation distance between the adjacent nodes of two trees)
                var maxMeltaDistance = 0.0;
                for (var level = 1; level <= maxLevel; level++)
                {
                    var deltaDistance = minDistance + siblingNodeRightContour[level] - nodeLeftContour[level];
                    if (deltaDistance > maxMeltaDistance)
                    {
                        maxMeltaDistance = deltaDistance;
                    }
                }

                if (maxMeltaDistance > 0)
                {
                    // need to shift node right to resolve collision with the tree on the left side
                    node.X += maxMeltaDistance;
                    node.ChildrenShiftX += maxMeltaDistance;
                    isNeedRecalculateNodeLeftContour = true;

                    // need to distribute (shift) all the nodes between current node and sibling node
                    this.DistributeEvenlyNodesBetween(leftNode: siblingNode, rightNode: node);
                }

                siblingNode = siblingNode.GetNextSibling();
            }
            while (siblingNode != node);
        }

        private class TreeLayoutNode
        {
            public readonly List<TreeLayoutNode> Children;

            /// <summary>
            /// How much we need to offset all the children nodes X position.
            /// It's known as "Mod" in Walker's algorithm.
            /// </summary>
            public double ChildrenShiftX;

            public TreeLayoutNode Parent;

            public double X;

            private readonly T originalNode;

            public TreeLayoutNode(T originalNode, int childrenCount)
            {
                this.originalNode = originalNode;
                this.Children = new List<TreeLayoutNode>(childrenCount);
            }

            public TreeLayoutNode GetLeftMostSibling()
            {
                return this.Parent is null
                           ? this
                           : this.Parent.Children[0];
            }

            public TreeLayoutNode GetNextSibling()
            {
                return this.Parent is null
                           ? this
                           : this.Parent.Children[this.Parent.Children.IndexOf(this) + 1];
            }

            public void SetLayoutPositionX(double nodeX)
            {
                this.originalNode?.SetLayoutPositionX(nodeX);
            }

            public override string ToString()
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                return this.originalNode?.ToString() ?? "<wrapper root node>";
            }
        }
    }
}