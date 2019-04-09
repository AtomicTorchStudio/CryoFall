namespace AtomicTorch.CBND.CoreMod.Characters.Input
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class CharacterMoveModesHelper
    {
        public static CharacterMoveModes CalculateMoveModes(Vector2D direction)
        {
            var mode = CharacterMoveModes.None;

            if (direction.X > 0)
            {
                mode |= CharacterMoveModes.Right;
            }
            else if (direction.X < 0)
            {
                mode |= CharacterMoveModes.Left;
            }

            if (direction.Y > 0)
            {
                mode |= CharacterMoveModes.Up;
            }
            else if (direction.Y < 0)
            {
                mode |= CharacterMoveModes.Down;
            }

            return mode;
        }
    }
}