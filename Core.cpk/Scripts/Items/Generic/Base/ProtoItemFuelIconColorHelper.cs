namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ProtoItemFuelIconColorHelper
    {
        private static readonly Color ColorElectricity
            = Api.Client.UI.GetApplicationResource<Color>("ColorElectricity");

        private static readonly Color ColorFuelOil
            = Api.Client.UI.GetApplicationResource<Color>("ColorFuelOil");

        private static readonly Color ColorFuelRefined
            = Api.Client.UI.GetApplicationResource<Color>("ColorFuelRefined");

        private static readonly Color ColorFuelSolid
            = Api.Client.UI.GetApplicationResource<Color>("ColorFuelSolid");

        private static readonly TextureResource IconElectricity = new("Icons/IconElectricity");

        private static readonly TextureResource IconFuelOil = new("Icons/Fuels/IconFuelOil");

        private static readonly TextureResource IconFuelRefined = new("Icons/Fuels/IconFuelRefined");

        private static readonly TextureResource IconFuelSolid = new("Icons/Fuels/IconFuelSolid");

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