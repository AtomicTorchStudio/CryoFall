namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    public static class WorldMapSectorProviderHelper
    {
        private static WorldMapSectorProvider providerEditorMap;

        private static WorldMapSectorProvider providerGameMap;

        public static WorldMapSectorProvider GetProvider(bool isEditor)
        {
            return isEditor
                       ? providerEditorMap ??= new WorldMapSectorProvider()
                       : providerGameMap ??= new WorldMapSectorProvider();
        }
    }
}