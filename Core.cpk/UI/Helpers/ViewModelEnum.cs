namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.GameEngine.Common.Extensions;

    public readonly struct ViewModelEnum<TValue> : IEquatable<ViewModelEnum<TValue>>
        where TValue : struct, Enum
    {
        public ViewModelEnum(TValue value)
        {
            this.Value = value;
        }

        public string Description => (this.Value as Enum).GetDescription();

        public int Order => (this.Value as Enum).GetDescriptionOrder();

        public TValue Value { get; }

        public static bool operator ==(ViewModelEnum<TValue> left, ViewModelEnum<TValue> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ViewModelEnum<TValue> left, ViewModelEnum<TValue> right)
        {
            return !left.Equals(right);
        }

        public bool Equals(ViewModelEnum<TValue> other)
        {
            return EqualityComparer<TValue>.Default.Equals(this.Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is ViewModelEnum<TValue> viewModelEnum
                   && this.Equals(viewModelEnum);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            return string.Format("ViewModel for enum entry: {0} ({1})",
                                 this.Value,
                                 this.Description);
        }
    }
}