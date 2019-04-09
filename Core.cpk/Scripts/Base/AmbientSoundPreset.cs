namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class AmbientSoundPreset : IEquatable<AmbientSoundPreset>
    {
        public AmbientSoundPreset(SoundResource soundResource, double suppression = 0)
        {
            this.SoundResource = soundResource;
            Api.Assert(suppression >= 0 && suppression <= 1, nameof(suppression) + " must be in [0;1] range");
            this.Suppression = suppression;
        }

        public SoundResource SoundResource { get; }

        public double Suppression { get; }

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
                   && this.Suppression.Equals(other.Suppression);
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
                       ^ this.Suppression.GetHashCode();
            }
        }
    }
}