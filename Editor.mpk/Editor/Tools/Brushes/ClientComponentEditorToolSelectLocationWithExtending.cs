namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentEditorToolSelectLocationWithExtending : BaseClientComponentEditorToolSelectLocation
    {
        private ExtendMode currentExtendMode = ExtendMode.None;

        private Vector2Ushort lastMouseTilePosition;

        private enum ExtendMode
        {
            None,

            Move,

            Left,

            Right,

            Up,

            Down,

            LeftDown,

            LeftUp,

            RightDown,

            RightUp
        }

        protected override void OnSelectionEnded()
        {
        }

        protected override void OnSelectionExtended()
        {
            if (this.currentExtendMode == ExtendMode.None)
            {
                base.OnSelectionExtended();
                return;
            }

            if (this.currentExtendMode == ExtendMode.Move)
            {
                this.ApplyMove();
                return;
            }

            // extending mode!
            var position = this.CurrentMouseTilePosition;

            // selection rectangle:
            // selection start - down-left corner
            // selection end - up-right corner
            switch (this.currentExtendMode)
            {
                case ExtendMode.Left:
                    this.ChangeSelectionStart(newX: position.X);
                    break;
                case ExtendMode.Right:
                    this.ChangeSelectionEnd(newX: position.X);
                    break;
                case ExtendMode.Up:
                    this.ChangeSelectionEnd(newY: position.Y);
                    break;
                case ExtendMode.Down:
                    this.ChangeSelectionStart(newY: position.Y);
                    break;
                case ExtendMode.LeftDown:
                    this.ChangeSelectionStart(newX: position.X);
                    this.ChangeSelectionStart(newY: position.Y);
                    break;
                case ExtendMode.LeftUp:
                    this.ChangeSelectionStart(newX: position.X);
                    this.ChangeSelectionEnd(newY: position.Y);
                    break;
                case ExtendMode.RightDown:
                    this.ChangeSelectionEnd(newX: position.X);
                    this.ChangeSelectionStart(newY: position.Y);
                    break;
                case ExtendMode.RightUp:
                    this.ChangeSelectionEnd(newX: position.X);
                    this.ChangeSelectionEnd(newY: position.Y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnSelectionStarted()
        {
            var position = this.CurrentMouseTilePosition;
            var selectionBounds = this.SelectionBounds;

            if (!selectionBounds.Contains(position))
            {
                // clicked outside - create new rectangle
                this.currentExtendMode = ExtendMode.None;
                base.OnSelectionStarted();
                return;
            }

            this.currentExtendMode = CalculateExtendMode(ref position, selectionBounds);
            //Logger.Dev("Selection rectangle extending started: " + this.currentExtendMode);

            // normalize start-end positions
            this.SelectionRectStartPosition = selectionBounds.Offset;
            this.SelectionRectEndPosition = (selectionBounds.Offset
                                             + selectionBounds.Size
                                             - Vector2Ushort.One).ToVector2Ushort();

            this.lastMouseTilePosition = position;
        }

        private static ExtendMode CalculateExtendMode(ref Vector2Ushort position, BoundsUshort bounds)
        {
            var minX = bounds.MinX;
            var minY = bounds.MinY;
            // calculate max XY (inclusive margins)
            var maxX = (ushort)(bounds.MaxX - 1);
            var maxY = (ushort)(bounds.MaxY - 1);

            if (position.X == minX
                && position.Y == minY)
            {
                return ExtendMode.LeftDown;
            }

            if (position.X == minX
                && position.Y == maxY)
            {
                return ExtendMode.LeftUp;
            }

            if (position.X == maxX
                && position.Y == minY)
            {
                return ExtendMode.RightDown;
            }

            if (position.X == maxX
                && position.Y == maxY)
            {
                return ExtendMode.RightUp;
            }

            // detect which side is closest
            var distanceToLeft = position.X - minX;
            var distanceToRight = maxX - position.X;
            var distanceToDown = position.Y - minY;
            var distanceToUp = maxY - position.Y;

            var minDistance = Math.Min(
                distanceToLeft,
                Math.Min(distanceToUp, Math.Min(distanceToRight, distanceToDown)));

            if (minDistance >= 1)
            {
                return ExtendMode.Move;
            }

            if (distanceToLeft == minDistance)
            {
                position = (minX, position.Y);
                return ExtendMode.Left;
            }

            if (distanceToRight == minDistance)
            {
                position = (maxX, position.Y);
                return ExtendMode.Right;
            }

            if (distanceToUp == minDistance)
            {
                position = (position.X, maxY);
                return ExtendMode.Up;
            }

            if (distanceToDown == minDistance)
            {
                position = (position.X, minY);
                return ExtendMode.Down;
            }

            throw new Exception("Impossible");
        }

        private void ApplyMove()
        {
            var delta = this.CurrentMouseTilePosition - (Vector2Int)this.lastMouseTilePosition;
            this.lastMouseTilePosition = this.CurrentMouseTilePosition;

            this.SelectionRectStartPosition = this.SelectionRectStartPosition.AddAndClamp(delta);
            this.SelectionRectEndPosition = this.SelectionRectEndPosition.AddAndClamp(delta);
        }

        private void ChangeSelectionEnd(ushort? newX = null, ushort? newY = null)
        {
            this.SelectionRectEndPosition = (newX ?? this.SelectionRectEndPosition.X,
                                             newY ?? this.SelectionRectEndPosition.Y);
        }

        private void ChangeSelectionStart(ushort? newX = null, ushort? newY = null)
        {
            this.SelectionRectStartPosition = (newX ?? this.SelectionRectStartPosition.X,
                                               newY ?? this.SelectionRectStartPosition.Y);
        }
    }
}