namespace AtomicTorch.CBND.CoreMod.Helpers.Primitives
{
    using System;

    [Serializable]
    public struct ColumnRow
    {
        public readonly byte Column;

        public readonly byte Row;

        public ColumnRow(byte column, byte row)
        {
            this.Row = row;
            this.Column = column;
        }
    }
}