namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Floors
{
    using System;

    public struct FloorSourceChunk : IEquatable<FloorSourceChunk>
    {
        public readonly byte Column;

        public readonly byte? MaskColumn;

        public readonly byte? MaskRow;

        public readonly byte Row;

        public FloorSourceChunk(byte column, byte row, byte? maskColumn = null, byte? maskRow = null)
        {
            this.Column = column;
            this.Row = row;
            this.MaskColumn = maskColumn;
            this.MaskRow = maskRow;
        }

        public bool Equals(FloorSourceChunk other)
        {
            return this.Column == other.Column
                   && this.MaskColumn == other.MaskColumn
                   && this.MaskRow == other.MaskRow
                   && this.Row == other.Row;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is FloorSourceChunk && this.Equals((FloorSourceChunk)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Column.GetHashCode();
                hashCode = (hashCode * 397) ^ this.MaskColumn.GetHashCode();
                hashCode = (hashCode * 397) ^ this.MaskRow.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Row.GetHashCode();
                return hashCode;
            }
        }
    }
}