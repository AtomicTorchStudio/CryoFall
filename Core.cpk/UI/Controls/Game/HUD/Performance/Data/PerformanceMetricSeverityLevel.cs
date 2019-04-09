namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance.Data
{
    using AtomicTorch.CBND.GameApi;

    [NotPersistent]
    public enum PerformanceMetricSeverityLevel : byte
    {
        None = 0,

        Green = 1,

        Yellow = 2,

        Red = 3
    }
}