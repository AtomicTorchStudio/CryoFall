namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public static class UITradingIcons
    {
        public static readonly Lazy<TextureBrush> LazyIconCoinPenny
            = new(() => Api.Client.UI.GetTextureBrush(
                      new TextureResource("Icons/Trading/CoinPenny.png")));

        public static readonly Lazy<TextureBrush> LazyIconCoinShiny
            = new(() => Api.Client.UI.GetTextureBrush(
                      new TextureResource("Icons/Trading/CoinShiny.png")));
    }
}