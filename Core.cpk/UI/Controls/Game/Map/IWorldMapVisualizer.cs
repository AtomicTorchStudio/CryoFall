namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;

    public interface IWorldMapVisualizer : IDisposable
    {
        bool IsEnabled { get; set; }
    }
}