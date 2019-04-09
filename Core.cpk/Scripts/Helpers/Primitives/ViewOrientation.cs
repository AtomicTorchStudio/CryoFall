namespace AtomicTorch.CBND.CoreMod.Helpers.Primitives
{
    using System;

    [Serializable]
    public struct ViewOrientation
    {
        /// <summary>
        /// It means the orientation is left half of circle.
        /// </summary>
        public bool IsLeft;

        /// <summary>
        /// It means the orientation is upper half of circle.
        /// </summary>
        public bool IsUp;

        public ViewOrientation(bool isUp, bool isLeft)
        {
            this.IsUp = isUp;
            this.IsLeft = isLeft;
        }
    }
}