namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Zones;

    public class ZoneScripts
    {
        private readonly HashSet<IZoneScriptConfig> scripts = new();

        public ZoneScripts Add(IZoneScriptConfig zoneScriptConfig)
        {
            this.scripts.Add(zoneScriptConfig);
            return this;
        }

        public ZoneScripts Add(IZoneScriptWithDefaultConfiguration zoneScriptWithDefaultConfiguration)
        {
            this.scripts.Add(zoneScriptWithDefaultConfiguration.DefaultConfiguration);
            return this;
        }

        public IReadOnlyList<IZoneScriptConfig> ToReadOnly()
        {
            return this.scripts.ToArray();
        }
    }
}