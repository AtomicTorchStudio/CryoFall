namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Structures;

    [Serializable]
    public readonly struct FactionEmblem : IEquatable<FactionEmblem>
    {
        public readonly string BackgroundId;

        public readonly ColorRgb ColorBackground1;

        public readonly ColorRgb ColorBackground2;

        public readonly ColorRgb ColorForeground;

        public readonly string ForegroundId;

        public readonly string ShapeMaskId;

        public FactionEmblem(
            string foregroundId,
            string backgroundId,
            string shapeMaskId,
            ColorRgb colorForeground,
            ColorRgb colorBackground1,
            ColorRgb colorBackground2)
        {
            this.ForegroundId = foregroundId;
            this.BackgroundId = backgroundId;
            this.ShapeMaskId = shapeMaskId;
            this.ColorForeground = colorForeground;
            this.ColorBackground1 = colorBackground1;
            this.ColorBackground2 = colorBackground2;
        }

        public bool Equals(FactionEmblem other)
        {
            return this.ForegroundId == other.ForegroundId
                   && this.BackgroundId == other.BackgroundId
                   && this.ShapeMaskId == other.ShapeMaskId
                   && this.ColorForeground.Equals(other.ColorForeground)
                   && this.ColorBackground1.Equals(other.ColorBackground1)
                   && this.ColorBackground2.Equals(other.ColorBackground2);
        }

        public override bool Equals(object obj)
        {
            return obj is FactionEmblem other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.ForegroundId,
                                    this.BackgroundId,
                                    this.ShapeMaskId,
                                    this.ColorForeground,
                                    this.ColorBackground1,
                                    this.ColorBackground2);
        }
    }
}