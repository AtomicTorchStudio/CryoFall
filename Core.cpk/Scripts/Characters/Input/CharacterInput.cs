namespace AtomicTorch.CBND.CoreMod.Characters.Input
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.GameEngine.Common.Primitives;

    public struct CharacterInput : IEquatable<CharacterInput>
    {
        private bool hasChanged;

        private CharacterMoveModes moveModes;

        // TODO: actually we can compress it to a single or two bytes for networking
        private float rotationAngleRad;

        public bool HasChanged => this.hasChanged;

        public CharacterMoveModes MoveModes
        {
            get => this.moveModes;
            set
            {
                if (this.moveModes == value)
                {
                    return;
                }

                this.moveModes = value;
                this.hasChanged = true;
            }
        }

        public float RotationAngleRad
        {
            get => this.rotationAngleRad;
            set
            {
                value %= MathConstants.DoublePI;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (this.RotationAngleRad == value)
                {
                    return;
                }

                this.rotationAngleRad = value;
                this.hasChanged = true;
            }
        }

        public static bool operator ==(CharacterInput left, CharacterInput right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CharacterInput left, CharacterInput right)
        {
            return !left.Equals(right);
        }

        public bool Equals(CharacterInput other)
        {
            return this.moveModes == other.moveModes
                   && this.rotationAngleRad.Equals(other.rotationAngleRad);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is CharacterInput
                   && this.Equals((CharacterInput)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)this.moveModes * 397)
                       ^ this.rotationAngleRad.GetHashCode();
            }
        }

        public void SetChanged(bool isChanged)
        {
            this.hasChanged = isChanged;
        }
    }
}