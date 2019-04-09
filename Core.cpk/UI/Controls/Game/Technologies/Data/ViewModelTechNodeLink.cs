namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelTechNodeLink : BaseViewModel
    {
        /// <summary>
        /// Bottom-left position.
        /// </summary>
        private readonly Vector2D position;

        private readonly ViewModelTechNode techNode;

        public ViewModelTechNodeLink(
            ViewModelTechNode techNode,
            double nodeWidth,
            double nodeHeight,
            double halfVerticalSpacing,
            double arrowHeight)
        {
            this.techNode = techNode;
            this.position = techNode.Position;
            this.position -= (0, arrowHeight);

            // calculate (local) target position
            var targetPosition = techNode.ParentNode.Position - this.position;

            // offset root position (it will also offset target position)
            this.position += (nodeWidth / 2, 0);

            // offset target position
            targetPosition += (0, nodeHeight);

            var path = new StreamGeometry();
            using (var ctx = path.Open())
            {
                ctx.BeginFigure(new Point(0, 0), false, false);

                if (targetPosition.X == 0)
                {
                    // target is at the same point - can directly go to it (straight line up)
                    ctx.LineTo(new Point(0, targetPosition.Y), false, false);
                }
                else
                {
                    // cannot directly link - create zig-zag shape
                    // calculate middle Y
                    var middleY = (targetPosition.Y + arrowHeight) / 2;

                    // go straight up to middle height
                    ctx.LineTo(new Point(0, middleY), false, false);
                    // go straight side keeping middle height
                    ctx.LineTo(new Point(targetPosition.X, middleY), false, false);
                    // go straight up to target height
                    ctx.LineTo(new Point(targetPosition.X, targetPosition.Y), false, false);
                }
            }

            this.Path = path;
            this.Refresh();

            this.techNode.IsUnlockedChanged += this.Refresh;
            this.techNode.CanUnlockChanged += this.Refresh;
        }

        public bool CanUnlock { get; private set; }

        public bool IsUnlocked { get; private set; }

        public StreamGeometry Path { get; }

        //public double X1 => this.targetPosition.X;

        //public double Y1 => this.targetPosition.Y;

        public double PositionX => this.position.X;

        public double PositionY => this.position.Y;

        public int PositionZ { get; set; }

        private void Refresh()
        {
            var isUnlocked = this.techNode.IsUnlocked;

            // unlocked node links should be displayed over locked
            this.PositionZ = isUnlocked ? 1 : 0;

            this.IsUnlocked = isUnlocked;

            if (isUnlocked)
            {
                this.CanUnlock = false;
                return;
            }

            this.CanUnlock = this.techNode.CanUnlock;
        }
    }
}