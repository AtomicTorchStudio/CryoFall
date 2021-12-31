namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ProtoItemWithCount : IEquatable<ProtoItemWithCount>
    {
        public ProtoItemWithCount(IProtoItem protoItem, ushort count)
        {
            this.ProtoItem = protoItem;
            this.Count = count;
        }

        public ushort Count { get; }

        public IProtoItem ProtoItem { get; }

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

            return this.Equals((ProtoItemWithCount)obj);
        }

        public bool Equals(ProtoItemWithCount other)
        {
            return ReferenceEquals(this.ProtoItem, other.ProtoItem)
                   && this.Count == other.Count;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Count.GetHashCode() * 397) ^ (this.ProtoItem?.GetHashCode() ?? 0);
            }
        }

        public override string ToString()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            return string.Format("ItemType: {0}, Count: {1}",
                                 this.ProtoItem,
                                 this.Count);
        }

        public ProtoItemWithCount WithRate(byte rateModifier)
        {
            return new(
                this.ProtoItem,
                count: (ushort)Math.Min(ushort.MaxValue, (ulong)this.Count * (ulong)rateModifier));
        }
    }
}