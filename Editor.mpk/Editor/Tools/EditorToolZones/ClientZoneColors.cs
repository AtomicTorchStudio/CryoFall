namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using System;
    using System.Windows.Media;

    public static class ClientZoneColors
    {
        private static readonly Color[] ZoneColors =
        {
            Colors.DeepSkyBlue,
            Color.FromRgb(158, 223, 58),
            Colors.OrangeRed,
            Colors.Cyan,
            Colors.Lime,
            Colors.Fuchsia,
            Colors.White,
            Color.FromRgb(40,  103, 81),
            Color.FromRgb(116, 98,  31),
            Color.FromRgb(50,  21,  145),
            Color.FromRgb(143, 15,  34),
            Color.FromRgb(2,   183, 206),
            Color.FromRgb(209, 140, 21),
            Color.FromRgb(242, 95,  11),
            Color.FromRgb(41,  239, 0),
            Color.FromRgb(0,   21,  246),
            Color.FromRgb(139, 137, 13),
            Color.FromRgb(66,  35,  35),
            Color.FromRgb(74,  6,   84),
            Color.FromRgb(171, 24,  95),
            Color.FromRgb(252, 207, 146),
            Color.FromRgb(252, 146, 234),
            Color.FromRgb(37,  6,   107),
            Color.FromRgb(225, 132, 177),
            Color.FromRgb(104, 133, 227),
            Color.FromRgb(67,  138, 52),
            Color.FromRgb(11,  195, 133)
        };

        public static Color GetZoneColor(byte zoneIndex)
        {
            return ZoneColors[Math.Min(zoneIndex, ZoneColors.Length - 1)];
        }
    }
}