namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data.TreeLayout
{
    using System.Collections.Generic;

    public interface ITreeNodeForLayout<T>
        where T : ITreeNodeForLayout<T>
    {
        IReadOnlyList<T> Children { get; }

        /// <summary>
        /// Set calculated layout X position.
        /// </summary>
        void SetLayoutPositionX(double x);
    }
}