namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class LiquidColorIconHelper
    {
        public static (Color color, ImageBrush icon) GetColorAndIcon(
            LiquidType? liquidType)
        {
            var ui = Api.Client.UI;

            var colorKey = "LiquidColor" + (liquidType?.ToString() ?? "Empty");
            var color = ui.GetApplicationResource<Color>(colorKey);

            ImageBrush icon;
            if (liquidType != null)
            {
                var iconKey = "LiquidIcon" + liquidType.Value;
                icon = ui.GetApplicationResource<ImageBrush>(iconKey);
            }
            else
            {
                icon = null;
            }

            return (color, icon);
        }

        public static ITextureResource GetTexture(LiquidType? liquidType)
        {
            return liquidType.HasValue
                       ? new TextureResource(
                           "Icons/Liquids/IconLiquid" + liquidType.Value)
                       : TextureResource.NoTexture;
        }
    }
}