namespace AtomicTorch.CBND.CoreMod.Characters.Input
{
    using System;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class CharacterMoveModesHelper
    {
        public static CharacterMoveModes GetPrevailingMoveModesForDirection(Vector2D direction)
        {
            if (direction.X == 0
                && direction.Y == 0)
            {
                return CharacterMoveModes.None;
            }

            var mode = CharacterMoveModes.None;
            var absX = Math.Abs(direction.X);
            var absY = Math.Abs(direction.Y);

            if (absX >= 0.5
                && absY >= 0.5)
            {
                // moving in both directions
                ApplyHorizontalMode(ref mode);
                ApplyVerticalMode(ref mode);
                return mode;
            }

            if (absX > absY)
            {
                // horizontal movement is prevailing
                ApplyHorizontalMode(ref mode);
                return mode;
            }

            // vertical movement is prevailing
            ApplyVerticalMode(ref mode);
            return mode;

            // ReSharper disable once VariableHidesOuterVariable
            void ApplyHorizontalMode(ref CharacterMoveModes mode)
            {
                if (direction.X > 0)
                {
                    mode |= CharacterMoveModes.Right;
                }
                else if (direction.X < 0)
                {
                    mode |= CharacterMoveModes.Left;
                }
            }

            // ReSharper disable once VariableHidesOuterVariable
            void ApplyVerticalMode(ref CharacterMoveModes mode)
            {
                if (direction.Y > 0)
                {
                    mode |= CharacterMoveModes.Up;
                }
                else if (direction.Y < 0)
                {
                    mode |= CharacterMoveModes.Down;
                }
            }
        }
    }
}