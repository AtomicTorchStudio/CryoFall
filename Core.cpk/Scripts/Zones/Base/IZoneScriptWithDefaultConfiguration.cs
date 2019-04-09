namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi.Data.Zones;

    public interface IZoneScriptWithDefaultConfiguration : IZoneScript
    {
        IZoneScriptConfig DefaultConfiguration { get; }
    }
}