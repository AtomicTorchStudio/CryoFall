namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class AmbientSoundPreset : IEquatable<AmbientSoundPreset>
    {
        /// <param name="soundResource">Sound resource with ambient sound.</param>
        /// <param name="suppressionCoef">How much other ambient sounds should be suppressed?</param>
        /// <param name="isSupressingMusic">Should music pause when hearing this ambient sound?</param>
        public AmbientSoundPreset(
            SoundResource soundResource,
            double suppressionCoef = 0,
            bool isSupressingMusic = false)
        {
            this.SoundResource = soundResource;
            Api.Assert(suppressionCoef >= 0 && suppressionCoef <= 1,
                       nameof(suppressionCoef) + " must be in [0;1] range");
            this.SuppressionCoef = suppressionCoef;
            this.IsSupressingMusic = isSupressingMusic;
        }

        public bool IsSupressingMusic { get; }

        public SoundResource SoundResource { get; }

        public double SuppressionCoef { get; }

        public bool Equals(AmbientSoundPreset other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.SoundResource.Equals(other.SoundResource)
                   && this.SuppressionCoef.Equals(other.SuppressionCoef);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((AmbientSoundPreset)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.SoundResource != null ? this.SoundResource.GetHashCode() : 0) * 397)
                       ^ this.SuppressionCoef.GetHashCode();
            }
        }
    }
}