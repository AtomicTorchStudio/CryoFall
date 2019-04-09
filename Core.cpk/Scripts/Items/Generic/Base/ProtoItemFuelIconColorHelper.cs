namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ProtoItemFuelIconColorHelper
    {
        private static readonly Color ColorElectricity = Color.FromRgb(0xFF, 0xEE, 0x00);

        private static readonly Color ColorFuelOil = Color.FromRgb(0xFF, 0x88, 0x00);

        private static readonly Color ColorFuelRefined = Color.FromRgb(0xFF, 0x66, 0x66);

        private static readonly Color ColorFuelSolid = Color.FromRgb(0xFF, 0x00, 0x00);

        private static readonly TextureResource IconElectricity = new TextureResource("Icons/IconElectricity");

        private static readonly TextureResource IconFuelOil = new TextureResource("Icons/Fuels/IconFuelOil");

        private static readonly TextureResource IconFuelRefined = new TextureResource("Icons/Fuels/IconFuelRefined");

        private static readonly TextureResource IconFuelSolid = new TextureResource("Icons/Fuels/IconFuelSolid");

        public static (ITextureResource icon, Color color) GetIconAndColor(Type fuelType)
        {
            if (typeof(IProtoItemFuelElectricity).IsAssignableFrom(fuelType))
            {
                return (IconElectricity, ColorElectricity);
            }

            if (typeof(IProtoItemFuelOil).IsAssignableFrom(fuelType))
            {
                return (IconFuelOil, ColorFuelOil);
            }

            if (typeof(IProtoItemFuelSolid).IsAssignableFrom(fuelType))
            {
                return (IconFuelSolid, ColorFuelSolid);
            }

            if (typeof(IProtoItemFuelRefined).IsAssignableFrom(fuelType))
            {
                return (IconFuelRefined, ColorFuelRefined);
            }

            Api.Logger.Error("No fuel icon is known for " + fuelType);
            return (TextureResource.NoTexture, Colors.White);
        }
    }
}