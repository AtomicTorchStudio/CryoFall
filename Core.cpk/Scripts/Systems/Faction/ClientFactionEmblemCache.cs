namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public static class ClientFactionEmblemCache
    {
        private static readonly Dictionary<string, FactionEmblem> CacheEmblems;

        private static readonly Dictionary<string, WeakReference<TextureBrush>> CacheTextureBrushes;

        static ClientFactionEmblemCache()
        {
            if (Api.IsServer)
            {
                return;
            }

            Api.Client.CurrentGame.ConnectionStateChanged += CurrentGameOnConnectionStateChanged;
            FactionSystem.ClientFactionDissolved += FactionSystemOnClientFactionDissolved;
            CacheEmblems = new Dictionary<string, FactionEmblem>();
            CacheTextureBrushes = new Dictionary<string, WeakReference<TextureBrush>>();
        }

        public static Brush GetEmblemTextureBrush(string clanTag)
        {
            if (CacheTextureBrushes.TryGetValue(clanTag, out var weakReferenceTextureBrush)
                && weakReferenceTextureBrush.TryGetTarget(out var textureBrush))
            {
                return textureBrush;
            }

            if (CacheEmblems.TryGetValue(clanTag, out var emblem))
            {
                var texture = ClientFactionEmblemTextureComposer.GetEmblemTexture(emblem, useCache: true);
                textureBrush = Api.Client.UI.GetTextureBrush(texture);
                CacheTextureBrushes[clanTag] = new WeakReference<TextureBrush>(textureBrush);
                return textureBrush;
            }

            // create placeholder texture brush, load the emblem async, and insert loaded emblem in the texture
            textureBrush = Api.Client.UI.CreateTextureBrush();
            CacheTextureBrushes[clanTag] = new WeakReference<TextureBrush>(textureBrush);
            LoadEmblemAsync(clanTag, textureBrush);
            return textureBrush;
        }

        private static void CurrentGameOnConnectionStateChanged()
        {
            CacheEmblems.Clear();
            CacheTextureBrushes.Clear();
        }

        private static void FactionSystemOnClientFactionDissolved(string clanTag)
        {
            CacheEmblems.Remove(clanTag);
            CacheTextureBrushes.Remove(clanTag);
        }

        private static async void LoadEmblemAsync(string clanTag, TextureBrush textureBrush)
        {
            var emblem = await FactionSystem.ClientGetFactionEmblemAsync(clanTag);
            CacheEmblems[clanTag] = emblem;

            var texture = ClientFactionEmblemTextureComposer.GetEmblemTexture(emblem, useCache: true);
            Api.Client.UI.SetTextureBrushResource(textureBrush, texture);
        }
    }
}