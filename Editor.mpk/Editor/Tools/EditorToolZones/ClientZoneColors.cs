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
            Colors.White
        };

        public static Color GetZoneColor(byte zoneIndex)
        {
            return ZoneColors[Math.Min(zoneIndex, ZoneColors.Length - 1)];
        }
    }
}