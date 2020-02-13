namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System.Windows.Media;

    public static class LandClaimZoneColors
    {
        public static readonly Color ZoneColorGraceArea
            = Color.FromArgb(0x70, 0x44, 0x44, 0x44);

        public static readonly Color ZoneColorNotOwnedByPlayer
            = Color.FromArgb(0x60, 0xDD, 0x00, 0x00);

        public static readonly Color ZoneColorOwnedByPlayer
            = Color.FromArgb(0x60, 0x00, 0x99, 0x00);
    }
}