namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentEditorToolSelectLocationFixedSize : BaseClientComponentEditorToolSelectLocation
    {
        private bool isMouseHeld;

        private Vector2Int lastWorldPosition;

        public override void Update(double deltaTime)
        {
            var worldPosition = (Vector2Int)ClientInputManager.MouseWorldPosition;
            var deltaPos = worldPosition - this.lastWorldPosition;
            this.lastWorldPosition = worldPosition;

            if (ClientInputManager.IsButtonDown(GameButton.ActionUseCurrentItem, evenIfHandled: true)
                && this.SelectionBounds.Contains(this.lastWorldPosition)
                && this.IsMouseOverSelectionRectange())
            {
                this.isMouseHeld = true;
                deltaPos = default;
            }

            if (!this.isMouseHeld)
            {
                return;
            }

            if (ClientInputManager.IsButtonUp(GameButton.ActionUseCurrentItem, evenIfHandled: true))
            {
                this.isMouseHeld = false;
            }

            var isMoved = deltaPos != default;
            if (!isMoved)
            {
                return;
            }

            this.SelectionRectStartPosition
                = (this.SelectionRectStartPosition.ToVector2Int() + deltaPos).ToVector2Ushort();

            this.SelectionRectEndPosition
                = (this.SelectionRectEndPosition.ToVector2Int() + deltaPos).ToVector2Ushort();

            this.UpdateSelectionRectange();
        }

        private bool IsMouseOverSelectionRectange()
        {
            var mouseScreenPosition = Api.Client.Input.MouseScreenPosition;
            var point = new Point(mouseScreenPosition.X, mouseScreenPosition.Y);
            var layoutRoot = Api.Client.UI.LayoutRoot;
            point = layoutRoot.PointFromScreen(point);
            var hitTest = VisualTreeHelper.HitTest(layoutRoot, point);
            return hitTest.VisualHit == this.SelectionRectange;
        }
    }
}