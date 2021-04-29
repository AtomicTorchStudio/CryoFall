namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public static class ClientFactionEmblemBrushCache
    {
        private static readonly Dictionary<string, WeakReference<TextureBrush>> CacheTextureBrushes;

        static ClientFactionEmblemBrushCache()
        {
            if (Api.IsServer)
            {
                return;
            }

            Api.Client.CurrentGame.ConnectionStateChanged += CurrentGameOnConnectionStateChanged;
            FactionSystem.ClientFactionDissolved += FactionSystemOnClientFactionDissolved;
            CacheTextureBrushes = new Dictionary<string, WeakReference<TextureBrush>>();
        }

        public static Brush GetEmblemTextureBrush(string clanTag)
        {
            if (string.IsNullOrEmpty(clanTag))
            {
                return Brushes.Magenta;
            }

            if (CacheTextureBrushes.TryGetValue(clanTag, out var weakReferenceTextureBrush)
                && weakReferenceTextureBrush.TryGetTarget(out var textureBrush))
            {
                return textureBrush;
            }

            if (ClientFactionEmblemDataCache.TryGetValue(clanTag, out var emblem))
            {
                var texture = ClientFactionEmblemTextureProvider.GetEmblemTexture(emblem, useCache: true);
                textureBrush = Api.Client.UI.GetTextureBrush(texture);
                CacheTextureBrushes[clanTag] = new WeakReference<TextureBrush>(textureBrush);
                return textureBrush;
            }

            // create placeholder texture brush, load the emblem async, and insert loaded emblem in the texture
            textureBrush = Api.Client.UI.CreateTextureBrush();
            CacheTextureBrushes[clanTag] = new WeakReference<TextureBrush>(textureBrush);
            LoadEmblemTextureAsync(clanTag, textureBrush);
            return textureBrush;
        }

        private static void CurrentGameOnConnectionStateChanged()
        {
            CacheTextureBrushes.Clear();
        }

        private static void FactionSystemOnClientFactionDissolved(string clanTag)
        {
            CacheTextureBrushes.Remove(clanTag);
        }

        private static async void LoadEmblemTextureAsync(string clanTag, TextureBrush textureBrush)
        {
            var emblem = await ClientFactionEmblemDataCache.GetFactionEmblemAsync(clanTag);
            var texture = ClientFactionEmblemTextureProvider.GetEmblemTexture(emblem, useCache: true);
            Api.Client.UI.SetTextureBrushResource(textureBrush, texture);
        }
    }
}